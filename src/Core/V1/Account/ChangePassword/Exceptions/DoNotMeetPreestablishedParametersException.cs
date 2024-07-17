using Core.Exceptions;

namespace Core.V1.Account.ChangePassword.Exceptions
{
    public class DoNotMeetPreestablishedParametersException : BusinessException
    {
        public DoNotMeetPreestablishedParametersException()
            : base("The password doesn't meet preestablished parameters.")
        {
        }
    }
}
