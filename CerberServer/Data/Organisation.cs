using System;
using System.Collections.Generic;

#nullable disable

namespace CerberServer.Data
{
    public partial class Organisation
    {
        public Organisation()
        {
            Operators = new HashSet<Operator>();
            Users = new HashSet<User>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string OrganisationKey { get; set; }
        public string Logo { get; set; }
        public bool Active { get; set; }

        public virtual ICollection<Operator> Operators { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
