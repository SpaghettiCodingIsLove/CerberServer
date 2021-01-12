using AutoMapper;
using CerberServer.Data;
using CerberServer.Helpers;
using CerberServer.Models.Accounts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace CerberServer.Services
{
    public class AccountService : IAccountService
    {
        private readonly CerberContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IEmailService _emailService;

        public AccountService(
            CerberContext context,
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _emailService = emailService;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            User account = _context.Users.SingleOrDefault(x => x.Email == model.Email);

            if (account == null || !BC.Verify(model.Password, account.Password))
            {
                throw new AppException("Email or password is incorrect");
            }

            // authentication successful so generate jwt and refresh tokens
            RefreshToken refreshToken = generateRefreshToken(account.Id);
            _context.RefreshTokens.Where(x => x.UserId == account.Id && x.IsActive).ToList().ForEach(x => x.IsActive = false);
            account.RefreshTokens.Add(refreshToken);

            // save changes to db
            _context.Update(account);
            _context.SaveChanges();

            AuthenticateResponse response = _mapper.Map<AuthenticateResponse>(account);
            response.RefreshToken = refreshToken.Token;
            response.Image = Convert.ToBase64String(File.ReadAllBytes(@$"C:\ProgramData\CerberServer\Images\{account.Image}.png"));
            return response;
        }

        public ExtendTokenResponse RefreshToken(string token)
        {
            (RefreshToken refreshToken, User account) = getRefreshToken(token);

            ExtendTokenResponse response;
            if (refreshToken.Expires >= DateTime.UtcNow)
            {
                refreshToken.Expires = DateTime.UtcNow.AddMinutes(5);
                response = new ExtendTokenResponse()
                {
                    Success = true
                };
            }
            else
            {
                refreshToken.IsActive = false;
                response = new ExtendTokenResponse()
                {
                    Success = false
                };
            }

            _context.Update(refreshToken);
            _context.SaveChanges();
            return response;
        }

        public void RevokeToken(string token)
        {
            (RefreshToken refreshToken, User account) = getRefreshToken(token);

            // revoke token and save
            refreshToken.IsActive = false;
            _context.Update(account);
            _context.SaveChanges();
        }

        public void Register(RegisterRequest model)
        {
            // ExtendTokenRequest
            if (_context.Users.Any(x => x.Email == model.Email))
            {
                // send already registered error in email to prevent account enumeration
                sendAlreadyRegisteredEmail(model.Email);
                return;
            }

            // map model to new account object
            User account = _mapper.Map<User>(model);

            // hash password
            account.Password = BC.HashPassword(model.Password);

            // save account
            if (!Directory.Exists(@"C:\ProgramData\CerberServer\Images"))
            {
                Directory.CreateDirectory(@"C:\ProgramData\CerberServer\Images");
            }

            string file = RandomString(20);
            while (_context.Users.Any(x => x.Image.Equals(file)))
            {
                file = RandomString(20);
            }
            File.WriteAllBytes(@$"C:\ProgramData\CerberServer\Images\{file}.png", Convert.FromBase64String(model.ImageArray));
            account.Image = file;
            account.Login = "";
            _context.Users.Add(account);
            _context.SaveChanges();

            // send email
            sendVerificationEmail(account);
        }

        public void ForgotPassword(ForgotPasswdRequest model)
        {
            User account = _context.Users.SingleOrDefault(x => x.Email == model.Email);

            // always return ok response to prevent email enumeration
            if (account == null) return;
        }

        public IEnumerable<AccountResponse> GetAll()
        {
            Microsoft.EntityFrameworkCore.DbSet<User> accounts = _context.Users;
            return _mapper.Map<IList<AccountResponse>>(accounts);
        }

        public AccountResponse GetById(int id)
        {
           User account = getAccount(id);
            return _mapper.Map<AccountResponse>(account);
        }

        public AccountResponse Update(int id, UpdateRequest model)
        {
            var account = getAccount(id);

            // validate
            if (account.Email != model.Email && _context.Users.Any(x => x.Email == model.Email))
                throw new AppException($"Email '{model.Email}' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                account.Password = BC.HashPassword(model.Password);

            // copy model to account and save
            _mapper.Map(model, account);
            _context.Users.Update(account);
            _context.SaveChanges();

            return _mapper.Map<AccountResponse>(account);
        }

        public void Delete(int id)
        {
            var account = getAccount(id);
            _context.Users.Remove(account);
            _context.SaveChanges();
        }

        public void ValidateResetToken(ExtendTokenRequest model)
        {
            var token = _context.RefreshTokens.SingleOrDefault(x =>
                x.Token == model.Token &&
                x.Expires >= DateTime.UtcNow &&
                x.IsActive);

            if (token == null)
                throw new AppException("Invalid token");
        }

        public List<UserResponse> GetUsersInOrganisation(int id)
        {
            User user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (!user.IsOperator)
            {
                throw new AppException("User is not operator");
            }

            if (user.OrganisationId.HasValue)
            {
                List<User> usersInOrganisation = _context.Users.Where(x => x.OrganisationId == user.OrganisationId.Value).ToList();
                List<RefreshToken> refreshTokens = _context.RefreshTokens.Where(x => usersInOrganisation.Select(x => x.Id).Contains(x.UserId) && x.IsActive && x.Expires >= DateTime.UtcNow).ToList();
                List<UserResponse> userResponses = new List<UserResponse>();

                foreach(User orgUser in usersInOrganisation)
                {
                    UserResponse currUser = new UserResponse();
                    _mapper.Map(orgUser, currUser);

                    RefreshToken token = refreshTokens.FirstOrDefault(x => x.UserId == user.Id);

                    if (token != null)
                    {
                        currUser.Online = true;
                    }
                    else
                    {
                        currUser.Online = false;
                    }

                    userResponses.Add(currUser);
                }

                return userResponses;
            }
            else
            {
                throw new AppException("User not in organisation");
            }
        }

        public OrganisationResponse GetOrganisation(int id)
        {
            User user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (user.OrganisationId.HasValue)
            {
                Organisation organisation = _context.Organisations.FirstOrDefault(x => x.Id == user.OrganisationId.Value);
                OrganisationResponse response = new OrganisationResponse();
                _mapper.Map(organisation, response);
                return response;
            }
            else
            {
                throw new AppException("User not in organisation");
            }
        }

        public void JoinOrganisation(JoinOrganisationRequest model)
        {
            Organisation organisation = _context.Organisations.FirstOrDefault(x => x.OrganisationKey.Equals(model.Key));

            if (organisation != null)
            {
                _context.Users.FirstOrDefault(x => x.Id == model.Id).OrganisationId = organisation.Id;
                _context.SaveChanges();
            }
            else
            {
                throw new AppException("Wrong key");
            }
        }

        // helper methods

        private User getAccount(int id)
        {
            var account = _context.Users.Find(id);
            if (account == null) throw new KeyNotFoundException("Account not found");
            return account;
        }

        private (RefreshToken, User) getRefreshToken(string token)
        {
            User account = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token && t.IsActive));
            if (account == null) throw new AppException("Invalid token");
            RefreshToken refreshToken = _context.RefreshTokens.Single(x => x.Token == token);
            if (!refreshToken.IsActive) throw new AppException("Invalid token");
            return (refreshToken, account);
        }

        private string generateJwtToken(User account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(long id)
        {
            return new RefreshToken
            {
                Token = randomTokenString(),
                Expires = DateTime.UtcNow.AddMinutes(5),
                IsActive = true,
                UserId = id
            };
        }

        private string randomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private void sendVerificationEmail(User account)
        {
            string message = $@"<p>Your account has been created!</p>";
            

            _emailService.Send(
                to: account.Email,
                subject: "Account creted!",
                html: $@"<h4>Verify Email</h4>
                         <p>Thanks for registering!</p>
                         {message}"
            );
        }

        private void sendAlreadyRegisteredEmail(string email)
        {
            

            _emailService.Send(
                to: email,
                subject: "Sign-up Verification API - Email Already Registered",
                html: $@"<h4>Email Already Registered</h4>
                         <p>Your email <strong>{email}</strong> is already registered.</p>"
            );
        }

        private string RandomString(int length)
        {

            string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder builder = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                char c = pool[random.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
