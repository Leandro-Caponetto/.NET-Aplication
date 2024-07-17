
namespace Core.V1.Account.CanExtendSession.Models
{
    public class CanExtendSessionResult
    {
        public CanExtendSessionResult(bool isValid)
        {
            IsValid = isValid;
        }

        public bool IsValid { get; }
    }
}
