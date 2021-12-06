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
    [Route("api/v1/movedex")]
    public class MovesController : StaticControllerBase
    {
        private static readonly IEnumerable<MoveModel> Moves = DexUtility.GetStaticDocuments<MoveModel>(DexType.Moves);

        [HttpGet]
        public StaticCollectionMessage FindMoves()
        {
            return GetStaticCollectionResponse(Moves);
        }

        [HttpGet("{name}")]
        public ActionResult<MoveModel> FindMove(string name)
        {
            var document = Moves.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
