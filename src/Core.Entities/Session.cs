using System;

namespace Core.Entities
{
    public class Session
    {
        public Session(string id, string userId, string token, DateTimeOffset expiresOn, DateTimeOffset createdOn)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Token = token ?? throw new ArgumentNullException(nameof(token));
            ExpiresOn = expiresOn;
            CreatedOn = createdOn;
            Active = true;
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
        public bool Active { get; set; }
    }
}
