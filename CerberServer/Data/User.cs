using System;
using System.Collections.Generic;

#nullable disable

namespace CerberServer.Data
{
    public partial class User
    {
        public User()
        {
            Operators = new HashSet<Operator>();
        }

        public long Id { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Image { get; set; }
        public long? OrganisationId { get; set; }
        public bool Active { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual ICollection<Operator> Operators { get; set; }
    }
}
