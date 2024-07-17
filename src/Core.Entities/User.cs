using Microsoft.AspNetCore.Identity;
using System;

namespace Core.Entities
{
    public class User : IdentityUser
    {
        public User() { }

        public User(string userName) : base(userName)
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}
