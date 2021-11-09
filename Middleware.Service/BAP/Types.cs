namespace Middleware.Service.BAP
{
    public static class Types
    {
        public static BAPStatus GetPaymentStatus(string status)
        {
            if (status.Equals("0"))
            {
                return BAPStatus.SUCCESS;
            }
            else if (status.Equals("1"))
            {
                return BAPStatus.PENDING;
            }
            return BAPStatus.FAILED;
        }
    }
}