﻿namespace CerberServer.Models.Accounts
{
    public class RevokeTokenRequest
    {
        public string Token { get; set; }
        public long Id { get; set; }
    }
}
