using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/classdex")]
    public class TrainerClassController : StaticControllerBase
    {
        private static readonly IEnumerable<TrainerClassModel> TrainerClasses = StaticDocumentUtility.GetStaticDocuments<TrainerClassModel>(StaticDocumentType.TrainerClasses);

        [HttpGet]
        public StaticCollectionMessage FindTrainerClasses()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(TrainerClasses);
        }

        [HttpGet("{name}")]
        public ActionResult<TrainerClassModel> FindTrainerClass(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = TrainerClasses.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
