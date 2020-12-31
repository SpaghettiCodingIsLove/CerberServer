using System;
using System.Collections.Generic;

#nullable disable

namespace CerberServer.Data
{
    public partial class User
    {
        public User()
        {
            RefreshTokens = new HashSet<RefreshToken>();
        }

        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Image { get; set; }
        public long? OrganisationId { get; set; }
        public bool IsActive { get; set; }
        public bool IsOperator { get; set; }
        public bool IsAdmin { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
