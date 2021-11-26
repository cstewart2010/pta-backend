using Microsoft.AspNetCore.Mvc;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/movedex")]
    public class MovesController : ControllerBase
    {
        [HttpGet("{name}")]
        public ActionResult<MoveModel> FindPokemon(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<MoveModel>(StaticDocumentType.Moves, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }
    }
}
