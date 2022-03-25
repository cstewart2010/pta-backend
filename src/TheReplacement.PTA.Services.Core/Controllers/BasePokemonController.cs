using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/pokedex")]
    public class BasePokemonController : StaticControllerBase
    {
        private static readonly IEnumerable<BasePokemonModel> BasePokemon = DexUtility.GetDexEntries<BasePokemonModel>(DexType.BasePokemon);

        [HttpGet]
        public StaticCollectionMessage FindPokemon()
        {
            return GetStaticCollectionResponse(BasePokemon.OrderBy(pokemon => pokemon.DexNo));
        }

        [HttpGet("{name}")]
        public ActionResult<BasePokemonModel> FindPokemon(string name)
        {
            var document = BasePokemon.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
