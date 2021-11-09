using System;
using System.Collections.Generic;
namespace Middleware.Service.FIServices
{
    public class GetBranchesResponse : BaseResponse
    {
        public IEnumerable<BranchInformation> Branches { get; set; }
    }

    public class BranchInformation
    {
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
    }
}
