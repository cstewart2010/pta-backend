using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/npc")]
    public class NpcController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public NpcController()
        {
            Collection = MongoCollection.Npcs;
        }

        [HttpGet("{npcId}")]
        public ActionResult<NpcModel> GetNpcs(string npcId)
        {
            if (string.IsNullOrEmpty(npcId))
            {
                return BadRequest(nameof(npcId));
            }

            var npc = DatabaseUtility.FindNpc(npcId);
            if (npc == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve npc {npcId}");
                return NotFound(npcId);
            }

            return ReturnSuccessfully(npc);
        }

        [HttpPost("new")]
        public ActionResult<NpcModel> CreateNewNpc()
        {
            var npc = CreateNpc(out var badResult);
            if (npc == null)
            {
                return badResult;
            }

            if (!DatabaseUtility.TryAddNpc(npc, out var error))
            {
                return BadRequest(error);
            }

            return ReturnSuccessfully(npc);
        }

        [HttpDelete("{npcId}")]
        public ActionResult DeleteNpc(string npcId)
        {
            if (string.IsNullOrEmpty(npcId))
            {
                return BadRequest(nameof(npcId));
            }

            if (!DatabaseUtility.DeleteNpc(npcId))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve npc {npcId}");
                return NotFound(npcId);
            }

            return ReturnSuccessfully(Ok());
        }

        private NpcModel CreateNpc(out ActionResult badRequest)
        {
            badRequest = null;
            if (!Request.Query.TryGetValue("trainerName", out var trainerName))
            {
                badRequest = BadRequest(new GenericMessage("Missing trainerName for npc"));
                return null;
            }

            var feats = Request.Query["feats"].ToString().Split(',')
                .Select(feat => DexUtility.GetStaticDocument<FeatureModel>(DexType.Features, feat))
                .Where(feat => feat != null)
                .Select(feat => feat.Name);

            var classes = Request.Query["classes"].ToString().Split(',')
                .Select(@class => DexUtility.GetStaticDocument<TrainerClassModel>(DexType.TrainerClasses, @class))
                .Where(@class => @class != null)
                .Select(@class => @class.Name);

            return new NpcModel
            {
                NPCId = Guid.NewGuid().ToString(),
                Feats = feats,
                TrainerClasses = classes,
                TrainerName = trainerName,
                TrainerStats = new StatsModel()
            };
        }
    }
}
