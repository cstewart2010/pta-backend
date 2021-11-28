using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/movedex")]
    public class MovesController : StaticControllerBase
    {
        private static readonly IEnumerable<MoveModel> Moves = StaticDocumentUtility.GetStaticDocuments<MoveModel>(StaticDocumentType.Moves);

        [HttpGet]
        public StaticCollectionMessage FindMoves()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(Moves);
        }

        [HttpGet("{name}")]
        public ActionResult<MoveModel> FindMove(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = Moves.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
