using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Core.DTO;
using Middleware.Service;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileManagersController : ControllerBase

    {
        private readonly IDocumentService  _documentService;
        public FileManagersController(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        [HttpGet("user-document-view")]
        public async Task<IActionResult> GetImage([FromHeader] string walletNumber, [FromHeader]DocumentType documentType,[FromHeader] string language)
        {
            var response = await _documentService.ViewDocument(walletNumber, documentType,language);
            if (response.IsSuccessful == true)
            {
                return Ok(response.GetPayload());
            }
            return BadRequest(response);
        }
    }
}

