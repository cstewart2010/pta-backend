using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/origindex")]
    public class OriginsController : StaticControllerBase
    {
        private static readonly IEnumerable<OriginModel> Origins = StaticDocumentUtility.GetStaticDocuments<OriginModel>(StaticDocumentType.Origins);

        [HttpGet]
        public StaticCollectionMessage FindOrigins()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(Origins);
        }

        [HttpGet("{name}")]
        public ActionResult<OriginModel> FindOrigin(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = Origins.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
