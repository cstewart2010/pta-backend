using Microsoft.AspNetCore.Mvc;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/itemdex")]
    public class ItemController : ControllerBase
    {
        [HttpGet("key/{name}")]
        public ActionResult<BaseItemModel> FindKeyItem(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<BaseItemModel>(StaticDocumentType.KeyItems, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }

        [HttpGet("medical/{name}")]
        public ActionResult<BaseItemModel> FindMedicalItem(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<BaseItemModel>(StaticDocumentType.MedicalItems, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }

        [HttpGet("pokeball/{name}")]
        public ActionResult<BaseItemModel> FindPokeball(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<BaseItemModel>(StaticDocumentType.Pokeballs, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }

        [HttpGet("pokemon/{name}")]
        public ActionResult<BaseItemModel> FindPokemonItem(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<BaseItemModel>(StaticDocumentType.PokemonItems, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }

        [HttpGet("trainer/{name}")]
        public ActionResult<BaseItemModel> FindTrainerEquipment(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<BaseItemModel>(StaticDocumentType.TrainerEquipment, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }
    }
}
