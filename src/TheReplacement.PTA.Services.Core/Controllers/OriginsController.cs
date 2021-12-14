using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Enums;
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
        private static readonly IEnumerable<OriginModel> Origins = DexUtility.GetDexEntries<OriginModel>(DexType.Origins);

        [HttpGet]
        public StaticCollectionMessage FindOrigins()
        {
            return GetStaticCollectionResponse(Origins);
        }

        [HttpGet("{name}")]
        public ActionResult<OriginModel> FindOrigin(string name)
        {
            var document = Origins.GetStaticDocument(name.Replace("_","/"));
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
