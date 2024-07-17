using System;

namespace Core.Shared.Services
{
    public interface IDateTimeOffsetService
    {
        DateTimeOffset UtcNow();
    }

    public class DateTimeOffsetService : IDateTimeOffsetService
    {
        public DateTimeOffset UtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
