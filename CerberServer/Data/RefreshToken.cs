using System;
using System.Collections.Generic;

#nullable disable

namespace CerberServer.Data
{
    public partial class RefreshToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsActive { get; set; }

        public virtual User User { get; set; }
    }
}
