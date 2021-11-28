using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        private static readonly IEnumerable<BaseItemModel> KeyItems = StaticDocumentUtility.GetStaticDocuments<BaseItemModel>(StaticDocumentType.KeyItems);
        private static readonly IEnumerable<BaseItemModel> MedicalItems = StaticDocumentUtility.GetStaticDocuments<BaseItemModel>(StaticDocumentType.MedicalItems);
        private static readonly IEnumerable<BaseItemModel> Pokeballs = StaticDocumentUtility.GetStaticDocuments<BaseItemModel>(StaticDocumentType.Pokeballs);
        private static readonly IEnumerable<BaseItemModel> PokemonItems = StaticDocumentUtility.GetStaticDocuments<BaseItemModel>(StaticDocumentType.PokemonItems);
        private static readonly IEnumerable<BaseItemModel> TrainerEquipment = StaticDocumentUtility.GetStaticDocuments<BaseItemModel>(StaticDocumentType.TrainerEquipment);

        [HttpGet("key")]
        public StaticCollectionMessage FindKeyItems()
        {
            return GetStaticCollectionResponse(KeyItems);
        }

        [HttpGet("key/{name}")]
        public ActionResult<BaseItemModel> FindKeyItem(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = KeyItems.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("medical")]
        public StaticCollectionMessage FindMedicalItems()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(MedicalItems);
        }

        [HttpGet("medical/{name}")]
        public ActionResult<BaseItemModel> FindMedicalItem(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = MedicalItems.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("pokeball")]
        public StaticCollectionMessage FindPokeballs()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(Pokeballs);
        }

        [HttpGet("pokeball/{name}")]
        public ActionResult<BaseItemModel> FindPokeball(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = Pokeballs.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("pokemon")]
        public StaticCollectionMessage FindPokemonItems()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(PokemonItems);
        }

        [HttpGet("pokemon/{name}")]
        public ActionResult<BaseItemModel> FindPokemonItem(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = PokemonItems.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("trainer")]
        public StaticCollectionMessage FindTrainerEquipment()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(TrainerEquipment);
        }

        [HttpGet("trainer/{name}")]
        public ActionResult<BaseItemModel> FindTrainerEquipment(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = TrainerEquipment.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
