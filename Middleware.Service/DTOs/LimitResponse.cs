namespace Middleware.Service.DTOs
{
    public class LimitResponse
    {
        public bool SingleLimitExceeded { get; set; }
        public bool DailyLimitExceeded { get; set; }
    }
}