using System;
using System.Collections.Generic;
namespace Middleware.Service.FIServices
{
    public class GetSubsidiariesResponse : BaseResponse
    {
        public IList<SubsidiaryDetail> Subsidiaries { get; set; }
    }

    public class SubsidiaryDetail
    {
        public string CountryId { get; set; }
        public string Name { get; set; }
        public string CurrencyCode { get; set; }
    }
}
