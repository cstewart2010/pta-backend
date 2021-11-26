using Microsoft.AspNetCore.Mvc;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/origindex")]
    public class OriginsController : ControllerBase
    {
        [HttpGet("{name}")]
        public ActionResult<OriginModel> FindOrigin(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<OriginModel>(StaticDocumentType.Origins, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }
    }
}
