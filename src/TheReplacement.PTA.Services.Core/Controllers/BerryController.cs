using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/berrydex")]
    public class BerryController : ControllerBase
    {
        [HttpGet("{name}")]
        public ActionResult<BerryModel> FindBerry(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<BerryModel>(StaticDocumentType.Berries, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }
    }
}
