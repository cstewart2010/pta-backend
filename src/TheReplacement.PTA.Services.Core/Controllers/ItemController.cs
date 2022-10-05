using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Interfaces;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/itemdex")]
    public class ItemController : StaticControllerBase
    {
        private static readonly IEnumerable<BaseItemModel> KeyItems = DexUtility.GetDexEntries<BaseItemModel>(DexType.KeyItems);
        private static readonly IEnumerable<BaseItemModel> MedicalItems = DexUtility.GetDexEntries<BaseItemModel>(DexType.MedicalItems);
        private static readonly IEnumerable<BaseItemModel> Pokeballs = DexUtility.GetDexEntries<BaseItemModel>(DexType.Pokeballs);
        private static readonly IEnumerable<BaseItemModel> PokemonItems = DexUtility.GetDexEntries<BaseItemModel>(DexType.PokemonItems);
        private static readonly IEnumerable<BaseItemModel> TrainerEquipment = DexUtility.GetDexEntries<BaseItemModel>(DexType.TrainerEquipment);

        [HttpGet("key")]
        public StaticCollectionMessage FindKeyItems()
        {
            return GetAlphabetizeStaticCollectionResponse(KeyItems);
        }

        [HttpGet("key/{name}")]
        public ActionResult<BaseItemModel> FindKeyItem(string name)
        {
            var document = KeyItems.GetStaticDocument(name.Replace("_", "/"));
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("medical")]
        public StaticCollectionMessage FindMedicalItems()
        {
            return GetAlphabetizeStaticCollectionResponse(MedicalItems);
        }

        [HttpGet("medical/{name}")]
        public ActionResult<BaseItemModel> FindMedicalItem(string name)
        {
            var document = MedicalItems.GetStaticDocument(name.Replace("_", "/"));
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("pokeball")]
        public StaticCollectionMessage FindPokeballs()
        {
            return GetAlphabetizeStaticCollectionResponse(Pokeballs);
        }

        [HttpGet("pokeball/{name}")]
        public ActionResult<BaseItemModel> FindPokeball(string name)
        {
            var document = Pokeballs.GetStaticDocument(name.Replace("_", "/"));
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("pokemon")]
        public StaticCollectionMessage FindPokemonItems()
        {
            return GetAlphabetizeStaticCollectionResponse(PokemonItems);
        }

        [HttpGet("pokemon/{name}")]
        public ActionResult<BaseItemModel> FindPokemonItem(string name)
        {
            var document = PokemonItems.GetStaticDocument(name.Replace("_", "/"));
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("trainer")]
        public StaticCollectionMessage FindTrainerEquipment()
        {
            return GetAlphabetizeStaticCollectionResponse(TrainerEquipment);
        }

        [HttpGet("trainer/{name}")]
        public ActionResult<BaseItemModel> FindTrainerEquipment(string name)
        {
            var document = TrainerEquipment.GetStaticDocument(name.Replace("_", "/"));
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        public StaticCollectionMessage GetAlphabetizeStaticCollectionResponse<TDocument>(IEnumerable<TDocument> documents) where TDocument : IDexDocument
        {
            if (!int.TryParse(Request.Query["offset"], out var offset))
            {
                offset = 0;
            }
            if (!int.TryParse(Request.Query["limit"], out var limit))
            {
                limit = 20;
            }

            var count = documents.Count();
            var previous = GetPreviousUrl(offset, limit);
            var next = GetNextUrl(offset, limit, count);
            var results = documents.GetSubset(offset, limit)
                .Select(GetResultsMember)
                .OrderBy(result => result.Name);

            return new StaticCollectionMessage
            (
                count,
                previous,
                next,
                results
            );
        }
    }
}
