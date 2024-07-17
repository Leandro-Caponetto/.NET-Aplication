
namespace Core.Shared
{
    public class LoggedRequest
    {
        private string userId;

        public string GetUserName()
        {
            return userId;
        }

        public void SetUserName(string userId)
        {
            this.userId = userId;
        }
    }
}
