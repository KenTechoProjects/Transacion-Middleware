using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Middleware.Client.Filters;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncryptDataController : RootController
    {
        private readonly IConfiguration conf;

        public EncryptDataController(IConfiguration conf)
        {
            this.conf = conf;
        }
        [AllowAnonymous]
        [HttpPost("encrypt-data")]
        public async Task<IActionResult> EncryptData(  [FromBody] dynamic request,string language)
        {
            var encrypt = string.Empty;
            try
            {
                var key = conf.GetValue<string>("SystemSettings:EncryptionKey");
                encrypt = await Util.Encryptor<dynamic>(request, key);
                return Ok(encrypt);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
      
        }

        [AllowAnonymous]
        [HttpPost("decrypt-data")]
        [TypeFilter(typeof(DecryptRequestDataFilter<dynamic>))]
        public async Task<IActionResult> DecryptData([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] dynamic request, string language)
        {
            var res = await Task.FromResult(request);
            return Ok(res);
        }
    }
}
