using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Site.Helpers.Models;
using System.Net;

namespace Presentation.Site.Controllers.Api.V1
{
    public class BaseController : ControllerBase
    {
        protected StatusCodeResult OkNoContent()
        {
            return StatusCode((int)HttpStatusCode.NoContent);
        }

        protected new OkObjectResult Ok(object value)
        {
            return base.Ok(new HttpSuccess(value));
        }
    }
}
