using AutoMapper;
using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.BaseServces
{
    public class MapperProfiles:Profile
    {
        public MapperProfiles()
        {

            CreateMap<Service.DTOs.AccountOpeningRequestForCustomerWithWallet, AccountOpeningRequest>();
        //        .ForMember(dest =>
        //    dest.Address,
        //    opt => opt.MapFrom(src => src.))
        //.ForMember(dest =>
        //    dest.LName,
        //    opt => opt.MapFrom(src => src.LastName));
           // CreateMap<AccountOpeningCompositRequest, AccountOpeningRequestForCustomerWithWallet>();
        }

    }
}
