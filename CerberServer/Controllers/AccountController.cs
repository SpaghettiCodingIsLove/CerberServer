using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using CerberServer.Data;
using CerberServer.Models.Accounts;
using CerberServer.Services;
using Microsoft.AspNetCore.Authorization;

namespace CerberServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public AccountController(
            IAccountService accountService,
            IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        [HttpPost("authenticate")]
        public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var response = _accountService.Authenticate(model);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public ActionResult<AuthenticateResponse> RefreshToken(ExtendTokenRequest extendToken)
        {
            var response = _accountService.RefreshToken(extendToken.Token, extendToken.Id);
            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken(RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token;

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            _accountService.RevokeToken(token, model.Id);
            return Ok(new { message = "Token revoked" });
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _accountService.Register(model);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswdRequest model)
        {
            _accountService.ForgotPassword(model);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        [HttpPost("validate-reset-token")]
        public IActionResult ValidateResetToken(ExtendTokenRequest model)
        {
            _accountService.ValidateResetToken(model);
            return Ok(new { message = "Token is valid" });
        }

        [HttpGet]
        public ActionResult<IEnumerable<AccountResponse>> GetAll()
        {
            var accounts = _accountService.GetAll();
            return Ok(accounts);
        }

        [HttpGet("{id:int}")]
        public ActionResult<AccountResponse> GetById(int id)
        {
            var account = _accountService.GetById(id);
            return Ok(account);
        }

        [HttpPut("{id:int}")] 
        public ActionResult<AccountResponse> Update(int id, UpdateRequest model)
        {
            var account = _accountService.Update(id, model);
            return Ok(account);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            _accountService.Delete(id);
            return Ok(new { message = "Account deleted successfully" });
        }

        [HttpGet("get-users")]
        public ActionResult<List<UserResponse>> GetUsersInOrganisation(string token, long id)
        {
            List<UserResponse> userRespnses = _accountService.GetUsersInOrganisation(token, id);
            return Ok(userRespnses);
        }

        [HttpGet("get-organisation")]
        public ActionResult<OrganisationResponse> GetOrganisation(string token, long id)
        {
            OrganisationResponse organisation = _accountService.GetOrganisation(token, id);
            return Ok(organisation);
        }

        [HttpPost("join-organisation")]
        public IActionResult JoinOrganisation(JoinOrganisationRequest model)
        {
            _accountService.JoinOrganisation(model);
            return Ok(new { message = "Joined organisation" });
        }
    }
}