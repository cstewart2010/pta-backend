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
    [Route("api/v1/pokedex")]
    public class BasePokemonController : StaticControllerBase
    {
        private static readonly IEnumerable<BasePokemonModel> BasePokemon = DexUtility.GetStaticDocuments<BasePokemonModel>(DexType.BasePokemon);

        [HttpGet]
        public StaticCollectionMessage FindPokemon()
        {
            return GetStaticCollectionResponse(BasePokemon);
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
