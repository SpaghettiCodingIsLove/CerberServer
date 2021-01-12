using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CerberServer.Models.Accounts
{
    public class UserResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Online { get; set; }
    }
}
