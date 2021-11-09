using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Client.Filters;
using Middleware.Core.DTO;
using Middleware.Service;
using Middleware.Service.DTOs;

namespace Middleware.Client.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : RootController
    {
        private readonly IDeviceService _service;

        public DeviceController(IDeviceService service)
        {
            _service = service;
        }

        [HttpGet, Route("status")]
        public async Task<IActionResult> GetDeviceStatus([FromHeader]string deviceId, [FromHeader]string language)
        {
            var response = await _service.GetDeviceStatus(deviceId, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok(response.GetPayload());
        }

        [HttpPost, Route("activate")]
        [TypeFilter(typeof(DecryptRequestDataFilter<DeviceActivationInitiationRequest>))]
     
        public async Task<IActionResult> InitiateActivation([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]DeviceActivationInitiationRequest request, [FromHeader] string language)
        {
            var response = await _service.InitiateDeviceActivation(request, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response);
        }


        [HttpPost, Route("activate-update")]//Was HttpPut Route("activate")
        [TypeFilter(typeof(DecryptRequestDataFilter<DeviceActivationCompletionRequest>))]
    
        public async Task<IActionResult> CompleteActivation([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]DeviceActivationCompletionRequest request, [FromHeader]string language)
        {
            var response = await _service.CompleteDeviceActivation(request, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response);
        }
       
    }
}
