using System;
using System.Collections.Generic;

#nullable disable

namespace CerberServer.Data
{
    public partial class Operator
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long OrganisationId { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual User User { get; set; }
    }
}
