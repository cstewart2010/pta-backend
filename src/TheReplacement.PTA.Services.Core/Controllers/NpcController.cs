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

        [HttpGet("{gameMasterId}/{npcId}")]
        public ActionResult<NpcModel> GetNpc(string gameMasterId, string npcId)
        {
            if(!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var npc = DatabaseUtility.FindNpc(npcId);
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (npc == null)
            {
                return NotFound(npcId);
            }

            // add check for npc and gameMaster's GameIds

            return npc;
        }

        [HttpPost("{gameMasterId}/new")]
        public async Task<ActionResult<NpcModel>> CreateNewNpcAsync(string gameMasterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            var npc = await CreateNpcAsync(gameMaster);
            if (!DatabaseUtility.TryAddNpc(npc, out var error))
            {
                return BadRequest(error);
            }

            var newNpcLog = new LogModel
            {
                User = npc.TrainerName,
                Action = $"has entered the chat at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), newNpcLog);
            Response.RefreshToken(gameMasterId);
            return npc;
        }

        [HttpDelete("{gameMasterId}/{npcId}")]
        public ActionResult DeleteNpc(string gameMasterId, string npcId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var npc = DatabaseUtility.FindNpc(npcId);
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (npc == null)
            {
                return NotFound(npcId);
            }

            // add check for npc and gameMaster's GameIds

            if (!DatabaseUtility.DeleteNpc(npcId))
            {
                return BadRequest(npcId);
            }

            var retiredNpcLog = new LogModel
            {
                User = npc.TrainerName,
                Action = $"has left the chat at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), retiredNpcLog);
            return Ok();
        }
        private async Task<NpcModel> CreateNpcAsync(TrainerModel gameMaster)
        {
            var json = await Request.GetRequestBody();
            var trainerName = json["trainerName"].ToString();

            var feats = json["feats"].Select(feat => DexUtility.GetDexEntry<FeatureModel>(DexType.Features, feat.ToString()))
                .Where(feat => feat != null)
                .Select(feat => feat.Name);

            var classes = json["classes"].Select(@class => DexUtility.GetDexEntry<TrainerClassModel>(DexType.TrainerClasses, @class.ToString()))
                .Where(@class => @class != null)
                .Select(@class => @class.Name);

            // add gameMaster's GameId to npc
            return new NpcModel
            {
                NPCId = Guid.NewGuid().ToString(),
                Feats = feats,
                TrainerClasses = classes,
                TrainerName = trainerName,
                TrainerStats = new StatsModel(),
                CurrentHP = 0
            };
        }
    }
}
