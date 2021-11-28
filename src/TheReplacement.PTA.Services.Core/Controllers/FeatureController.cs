using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        private static readonly IEnumerable<FeatureModel> GeneralFeatures = StaticDocumentUtility.GetStaticDocuments<FeatureModel>(StaticDocumentType.Features);
        private static readonly IEnumerable<FeatureModel> LegendaryFeatures = StaticDocumentUtility.GetStaticDocuments<FeatureModel>(StaticDocumentType.LegendaryFeatures);
        private static readonly IEnumerable<FeatureModel> Passives = StaticDocumentUtility.GetStaticDocuments<FeatureModel>(StaticDocumentType.Passives);
        private static readonly IEnumerable<FeatureModel> Skills = StaticDocumentUtility.GetStaticDocuments<FeatureModel>(StaticDocumentType.Skills);

        [HttpGet("general")]
        public StaticCollectionMessage FindGeneralFeatures()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(GeneralFeatures);
        }

        [HttpGet("general/{name}")]
        public ActionResult<FeatureModel> FindGeneralFeature(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
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
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(LegendaryFeatures);
        }

        [HttpGet("legendary/{name}")]
        public ActionResult<FeatureModel> FindLegendaryFeature(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
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
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(Passives);
        }

        [HttpGet("passives/{name}")]
        public ActionResult<FeatureModel> FindPassive(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
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
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            return GetStaticCollectionResponse(Skills);
        }

        [HttpGet("skills/{name}")]
        public ActionResult<FeatureModel> FindSkill(string name)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = Skills.GetStaticDocument(name);
            if (document != null)
            {
                return document;
            }

            return NotFound(name);
        }
    }
}
