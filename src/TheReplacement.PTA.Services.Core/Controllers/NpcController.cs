using Microsoft.AspNetCore.Mvc;
using System;
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
            Collection = MongoCollection.Npc;
        }

        [HttpGet("{npcId}")]
        public ActionResult<NpcModel> GetNpcs(string npcId)
        {
            Response.UpdateAccessControl();
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
            Response.UpdateAccessControl();
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
            Response.UpdateAccessControl();
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

            return new NpcModel
            {
                NPCId = Guid.NewGuid().ToString(),
                Feats = Request.Query["feats"].ToString().Split(','),
                TrainerClasses = Request.Query["classes"].ToString().Split(','),
                TrainerName = trainerName,
                TrainerStats = new TrainerStatsModel()
            };
        }
    }
}
