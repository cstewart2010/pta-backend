using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Utilities;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/npc")]
    public class NpcController : ControllerBase
    {
        private readonly ILogger<NpcController> _logger;

        public NpcController(ILogger<NpcController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{npcId}")]
        public ActionResult<NpcModel> GetNpcs(string npcId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var npc = DatabaseUtility.FindNpcs(new[] { npcId }).SingleOrDefault();
            if (npc == null)
            {
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
                Feats = Request.Query["feats"].ToString()?.Split(',') ?? Array.Empty<string>(),
                TrainerClasses = Request.Query["classes"].ToString()?.Split(',') ?? Array.Empty<string>(),
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

            return NotFound(npcId);
        }
    }
}
