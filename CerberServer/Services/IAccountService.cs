using CerberServer.Models.Accounts;
using System.Collections.Generic;

namespace CerberServer.Services
{
    public interface IAccountService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        ExtendTokenResponse RefreshToken(string token);
        void RevokeToken(string token);
        void Register(RegisterRequest model);
        void ForgotPassword(ForgotPasswdRequest model);
        void ValidateResetToken(ExtendTokenRequest model);
        IEnumerable<AccountResponse> GetAll();
        AccountResponse GetById(int id);
        AccountResponse Update(int id, UpdateRequest model);
        void Delete(int id);
        List<UserResponse> GetUsersInOrganisation(int id);
        OrganisationResponse GetOrganisation(int id);
        void JoinOrganisation(JoinOrganisationRequest model);
    }
}
