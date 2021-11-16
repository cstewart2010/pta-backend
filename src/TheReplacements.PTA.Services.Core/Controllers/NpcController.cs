using Microsoft.AspNetCore.Mvc;
using System;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Utilities;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/npc")]
    public class NpcController : ControllerBase
    {
        private const MongoCollection Collection = MongoCollection.Npc;

        private string ClientIp => Request.HttpContext.Connection.RemoteIpAddress.ToString();

        [HttpGet("{npcId}")]
        public ActionResult<NpcModel> GetNpcs(string npcId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var npc = DatabaseUtility.FindNpc(npcId);
            if (npc == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve npc {npcId}");
                return NotFound(npcId);
            }

            return npc;
        }

        [HttpPost("new")]
        public ActionResult<NpcModel> CreateNewNpc()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
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

            if (DatabaseUtility.TryAddNpc(npc, out var error))
            {
                return npc;
            }

            return BadRequest(error);
        }

        [HttpDelete("{npcId}")]
        public ActionResult DeleteNpc(string npcId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (DatabaseUtility.DeleteNpc(npcId))
            {
                return Ok();
            }

            LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve npc {npcId}");
            return NotFound(npcId);
        }
    }
}
