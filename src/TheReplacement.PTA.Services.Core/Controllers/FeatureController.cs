using Microsoft.AspNetCore.Mvc;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/featuredex")]
    public class FeatureController : ControllerBase
    {
        [HttpGet("general/{name}")]
        public ActionResult<FeatureModel> FindGeneralFeature(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<FeatureModel>(StaticDocumentType.Features, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }

        [HttpGet("legendary/{name}")]
        public ActionResult<FeatureModel> FindLegendaryFeature(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<FeatureModel>(StaticDocumentType.LegendaryFeatures, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }

        [HttpGet("passives/{name}")]
        public ActionResult<FeatureModel> FindPassiveFeature(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<FeatureModel>(StaticDocumentType.Passives, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }

        [HttpGet("skills/{name}")]
        public ActionResult<FeatureModel> FindSkillsFeature(string name)
        {
            Response.UpdateAccessControl();
            var document = StaticDocumentUtility.GetStaticDocument<FeatureModel>(StaticDocumentType.Skills, name);
            if (document == null)
            {
                return NotFound(name);
            }

            return document;
        }
    }
}
