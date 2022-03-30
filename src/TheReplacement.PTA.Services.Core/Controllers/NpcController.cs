using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
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
        public ActionResult<NpcModel> GetNpc(string npcId)
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

            return npc;
        }

        [HttpPost("new")]
        public async Task<ActionResult<NpcModel>> CreateNewNpcAsync()
        {
            var npc = await CreateNpcAsync();
            if (npc == null)
            {
                throw new Exception();
            }

            if (!DatabaseUtility.TryAddNpc(npc, out var error))
            {
                return BadRequest(error);
            }

            return npc;
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

            return Ok();
        }

        private async Task<NpcModel> CreateNpcAsync()
        {
            var json = await Request.GetRequestBody();
            var trainerName = json["trainerName"].ToString();

            var feats = json["feats"].Select(feat => DexUtility.GetDexEntry<FeatureModel>(DexType.Features, feat.ToString()))
                .Where(feat => feat != null)
                .Select(feat => feat.Name);

            var classes = json["classes"].Select(@class => DexUtility.GetDexEntry<TrainerClassModel>(DexType.TrainerClasses, @class.ToString()))
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
