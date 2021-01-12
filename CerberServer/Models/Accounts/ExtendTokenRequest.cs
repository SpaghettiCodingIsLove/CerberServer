using System.ComponentModel.DataAnnotations;

namespace CerberServer.Models.Accounts
{
    public class ExtendTokenRequest
    {
        [Required]
        public string Token { get; set; }
        public long Id { get; set; }
    }
}
