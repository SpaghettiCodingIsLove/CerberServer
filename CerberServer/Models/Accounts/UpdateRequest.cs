﻿using System.ComponentModel.DataAnnotations;

namespace CerberServer.Models.Accounts
{
    public class UpdateRequest
    {
        private string _password;
        private string _email;

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [EmailAddress]
        public string Email
        {
            get => _email;
            set => _email = replaceEmptyWithNull(value);
        }

        public string Password
        {
            get => _password;
            set => _password = replaceEmptyWithNull(value);
        }

        private string replaceEmptyWithNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
