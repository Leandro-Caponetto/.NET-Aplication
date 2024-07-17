using System;

namespace Core.V1.Account.Get.Models
{
    public class AccountModel
    {
        public AccountModel(string id, string email, string firstName, string lastName, bool emailConfirmed)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Email = email;
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            EmailConfirmed = emailConfirmed;
        }

        public string Id { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public bool EmailConfirmed { get; }
    }
}
