using Microsoft.AspNetCore.Mvc;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/pokedex")]
    public class BasePokemonController : ControllerBase
    {
        [HttpGet("{name}")]
        public ActionResult<BasePokemonModel> FindPokemon(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<BasePokemonModel>(StaticDocumentType.BasePokemon, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }
    }
}
