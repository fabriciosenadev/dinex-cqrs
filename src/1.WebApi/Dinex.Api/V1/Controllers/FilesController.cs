
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dinex.Api.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class FilesController : MainController
    {
        private readonly IMediator _mediator;

        public FilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> UploadFile(UploadFileCommand command)
        {
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
