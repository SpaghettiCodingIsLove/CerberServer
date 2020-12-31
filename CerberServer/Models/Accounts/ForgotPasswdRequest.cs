using System.ComponentModel.DataAnnotations;

namespace CerberServer.Models.Accounts
{
    public class ForgotPasswdRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
