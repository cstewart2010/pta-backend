using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/berrydex")]
    public class BerryController : StaticControllerBase
    {
        private static readonly IEnumerable<BerryModel> Berries = StaticDocumentUtility.GetStaticDocuments<BerryModel>(StaticDocumentType.Berries);

        [HttpGet]
        public StaticCollectionMessage FindBerries()
        {
            Response.UpdateAccessControl();
            return GetStaticCollectionResponse(Berries);
        }

        [HttpGet("{name}")]
        public ActionResult<BerryModel> FindBerry(string name)
        {
            Response.UpdateAccessControl();
            var document = Berries.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
