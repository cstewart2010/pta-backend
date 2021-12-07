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
    [Route("api/v1/featuredex")]
    public class FeatureController : StaticControllerBase
    {
        private static readonly IEnumerable<FeatureModel> GeneralFeatures = DexUtility.GetDexEntries<FeatureModel>(DexType.Features);
        private static readonly IEnumerable<FeatureModel> LegendaryFeatures = DexUtility.GetDexEntries<FeatureModel>(DexType.LegendaryFeatures);
        private static readonly IEnumerable<FeatureModel> Passives = DexUtility.GetDexEntries<FeatureModel>(DexType.Passives);
        private static readonly IEnumerable<FeatureModel> Skills = DexUtility.GetDexEntries<FeatureModel>(DexType.Skills);

        [HttpGet("general")]
        public StaticCollectionMessage FindGeneralFeatures()
        {
            return GetStaticCollectionResponse(GeneralFeatures);
        }

        [HttpGet("general/{name}")]
        public ActionResult<FeatureModel> FindGeneralFeature(string name)
        {
            var document = GeneralFeatures.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("legendary")]
        public StaticCollectionMessage FindLegendaryFeatures()
        {
            return GetStaticCollectionResponse(LegendaryFeatures);
        }

        [HttpGet("legendary/{name}")]
        public ActionResult<FeatureModel> FindLegendaryFeature(string name)
        {
            var document = LegendaryFeatures.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("passives")]
        public StaticCollectionMessage FindPassives()
        {
            return GetStaticCollectionResponse(Passives);
        }

        [HttpGet("passives/{name}")]
        public ActionResult<FeatureModel> FindPassive(string name)
        {
            var document = Passives.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }

        [HttpGet("skills")]
        public StaticCollectionMessage FindSkills()
        {
            return GetStaticCollectionResponse(Skills);
        }

        [HttpGet("skills/{name}")]
        public ActionResult<FeatureModel> FindSkill(string name)
        {
            var document = Skills.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
