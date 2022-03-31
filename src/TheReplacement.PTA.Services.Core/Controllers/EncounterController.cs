using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    [Route("api/v1/encounter")]
    public class EncounterController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public EncounterController()
        {
            Collection = MongoCollection.Encounter;
        }

        [HttpGet("{gameId}")]
        public ActionResult<EncounterModel> GetActiveEncounter(string gameId)
        {
            return DatabaseUtility.FindActiveEncounter(gameId);
        }

        [HttpGet("{gameMasterId}/all")]
        public ActionResult<IEnumerable<EncounterModel>> GetAllEncounters(string gameMasterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            return DatabaseUtility.FindAllEncounters(gameMaster.GameId).ToList();
        }

        [HttpPost("{gameMasterId}")]
        public async Task<ActionResult> CreateEncounterAsync(string gameMasterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var (name, type, message) = await GetCreationParametersAsync();
            if (message != null)
            {
                return BadRequest(message);
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            var encounter = new EncounterModel
            {
                EncounterId = Guid.NewGuid().ToString(),
                GameId = gameMaster.GameId,
                Name = name,
                Type = type,
                ActiveParticipants = Array.Empty<EncounterParticipantModel>()
            };

            var (addResult, error) = DatabaseUtility.TryAddEncounter(encounter);
            if (!addResult)
            {
                return BadRequest(error);
            }

            return Ok();
        }

        [HttpPut("{trainerId}")]
        public async Task<ActionResult> AddToActiveEncounterAsync(string trainerId)
        {
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            var encounter = DatabaseUtility.FindActiveEncounter(trainer.GameId);
            if (encounter == null)
            {
                return NotFound();
            }

            var json = await Request.GetRequestBody();
            var participant = json.ToObject<EncounterParticipantModel>();
            encounter.ActiveParticipants = encounter.ActiveParticipants.Append(participant);
            if (DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut("{gameMasterId}/active/{encounterId}")]
        public ActionResult SetEncounterToActive(string gameMasterId, string encounterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (DatabaseUtility.FindActiveEncounter(gameMaster.GameId) != null)
            {
                return Conflict();
            }

            var encounter = DatabaseUtility.FindEncounter(encounterId);
            if (encounter == null)
            {
                return NotFound(encounterId);
            }

            encounter.IsActive = true;
            if (DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut("{gameMasterId}/inactive/{encounterId}")]
        public ActionResult SetEncounterToInactive(string gameMasterId, string encounterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindEncounter(encounterId);
            if (encounter == null)
            {
                return NotFound(encounterId);
            }

            encounter.IsActive = false;
            if (DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut("{gameMasterId}/hp/{encounterId}")]
        public ActionResult UpdateParticipantsHp(string gameMasterId, string encounterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindEncounter(encounterId);
            if (encounter == null)
            {
                return NotFound(encounterId);
            }

            encounter.ActiveParticipants = encounter.ActiveParticipants.Select(GetWithUpdatedHP);
            if (DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{gameMasterId}/{encounterId}")]
        public ActionResult DeleteEncounter(string gameMasterId, string encounterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteEncounter(encounterId))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{gameMasterId}")]
        public ActionResult DeleteEncounters(string gameMasterId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (!DatabaseUtility.DeleteEncountersByGameId(gameMaster.GameId))
            {
                return BadRequest();
            }

            return Ok();
        }

        private static EncounterParticipantModel GetWithUpdatedHP(EncounterParticipantModel participant)
        {
            return Enum.Parse<EncounterParticipantType>(participant.Type, true) switch
            {
                EncounterParticipantType.Trainer => EncounterParticipantModel.FromTrainer(participant.Id),
                EncounterParticipantType.Pokemon => EncounterParticipantModel.FromPokemon(participant.Id),
                EncounterParticipantType.Npc => EncounterParticipantModel.FromNpc(participant.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(participant.Type)),
            };
        }

        private async Task<(string Name, string Type, AbstractMessage Message)> GetCreationParametersAsync()
        {
            var json = await Request.GetRequestBody();
            if (json["name"] == null)
            {
                return (null, null, new GenericMessage($"Missing Body parameter: name"));
            }
            if (json["type"] == null)
            {
                return (null, null, new GenericMessage($"Missing Body parameter: type"));
            }
            var name = json["name"].ToString();
            var type = json["type"].ToString();
            if (string.IsNullOrWhiteSpace(name) || name.Length > 30)
            {
                return (null, null, new GenericMessage("Make sure the name parameter is between 1 and 30 characters"));
            }
            if (!Enum.TryParse<EncounterType>(type, true, out _))
            {
                return (null, null, new GenericMessage($"Make sure the type parameter is one of {string.Join(',', Enum.GetNames(typeof(EncounterType)))}"));
            }

            return (name, type, null);
        }
    }
}
