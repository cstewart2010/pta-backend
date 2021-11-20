using Microsoft.AspNetCore.Mvc;
using System;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Services.Core.Extensions;

namespace TheReplacements.PTA.Services.Core.Controllers
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
            var npc = new NpcModel
            {
                NPCId = Guid.NewGuid().ToString(),
                Feats = Request.Query["feats"].ToString().Split(','),
                TrainerClasses = Request.Query["classes"].ToString().Split(','),
                TrainerName = Request.Query["trainerName"],
                TrainerStats = new TrainerStatsModel()
            };

            if (string.IsNullOrWhiteSpace(npc.TrainerName))
            {
                return BadRequest(new
                {
                    message = "Missing trainerName for npc"
                });
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
    }
}
