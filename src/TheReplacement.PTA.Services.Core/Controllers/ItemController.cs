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
            return GetStaticCollectionResponse(KeyItems);
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
            return GetStaticCollectionResponse(MedicalItems);
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
            return GetStaticCollectionResponse(Pokeballs);
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
            return GetStaticCollectionResponse(PokemonItems);
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
            return GetStaticCollectionResponse(TrainerEquipment);
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
    }
}
