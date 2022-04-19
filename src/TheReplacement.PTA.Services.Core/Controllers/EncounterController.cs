using Microsoft.AspNetCore.Mvc;
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
            Collection = MongoCollection.Encounters;
        }

        [HttpGet("{gameId}")]
        public ActionResult<EncounterModel> GetActiveEncounter(string gameId)
        {
            return DatabaseUtility.FindActiveEncounter(gameId) ?? new EncounterModel();
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
            if (encounter.ActiveParticipants.Any(activeParticipant => activeParticipant.ParticipantId == participant.ParticipantId))
            {
                return Conflict();
            }
            if (encounter.ActiveParticipants.Any(activeParticipant => 
            {
                if (activeParticipant.Position.X == participant.Position.X)
                {
                    if (activeParticipant.Position.Y == participant.Position.Y)
                    {
                        return true;
                    }
                }

                return false;
            }))
            {
                return Conflict();
            }
            encounter.ActiveParticipants = encounter.ActiveParticipants.Append(participant);
            if (!DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut("{gameMasterId}/position/{participantId}")]
        public async Task<ActionResult> UpdatePositionAsync(string gameMasterId, string participantId)
        {
            if (!Request.VerifyIdentity(gameMasterId, false))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            var encounter = DatabaseUtility.FindActiveEncounter(gameMaster.GameId);
            if (encounter == null)
            {
                return NotFound();
            }

            var json = await Request.GetRequestBody();
            var position = (await Request.GetRequestBody()).ToObject<MapPositionModel>();
            if (encounter.ActiveParticipants.Any(activeParticipant =>
            {
                if (activeParticipant.Position.X == position.X)
                {
                    if (activeParticipant.Position.Y == position.Y)
                    {
                        return true;
                    }
                }

                return false;
            }))
            {
                return Conflict();
            }
            var participant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == participantId);
            encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant =>
            {
                if (participant.ParticipantId == participantId)
                {
                    participant.Position = position;
                }

                return participant;
            });

            if (!DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            SendRepositionLog(gameMaster.GameId, participant.Name, position);
            return Ok();
        }

        [HttpPut("{trainerId}/trainer_position")]
        public async Task<ActionResult> UpdateTrainerPositionAsync(string trainerId)
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

            var position = (await Request.GetRequestBody()).ToObject<MapPositionModel>();
            var participant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == trainerId);
            if (GetDistance(position, participant.Position) > participant.Speed)
            {
                return StatusCode(411);
            }
            if (encounter.ActiveParticipants.Any(activeParticipant =>
            {
                if (activeParticipant.Position.X == position.X)
                {
                    if (activeParticipant.Position.Y == position.Y)
                    {
                        return true;
                    }
                }

                return false;
            }))
            {
                return Conflict();
            }
            encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant =>
            {
                if (participant.ParticipantId == trainerId)
                {
                    participant.Position = position;
                }

                return participant;
            });

            if (!DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            SendRepositionLog(trainer.GameId, participant.Name, position);
            return Ok();
        }

        [HttpPut("{trainerId}/pokemon_position/{pokemonId}")]
        public async Task<ActionResult> UpdateTrainerPokemonPositionAsync(string trainerId, string pokemonId)
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

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon.TrainerId != trainerId)
            {
                return Conflict();
            }

            var position = (await Request.GetRequestBody()).ToObject<MapPositionModel>();
            var participant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == pokemonId);
            if (GetDistance(position, participant.Position) > participant.Speed)
            {
                return StatusCode(411);
            }
            if (encounter.ActiveParticipants.Any(activeParticipant =>
            {
                if (activeParticipant.Position.X == position.X)
                {
                    if (activeParticipant.Position.Y == position.Y)
                    {
                        return true;
                    }
                }

                return false;
            }))
            {
                return Conflict();
            }
            encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant =>
            {
                if (participant.ParticipantId == pokemonId)
                {
                    participant.Position = position;
                }

                return participant;
            });

            if (!DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            SendRepositionLog(trainer.GameId, participant.Name, position);
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
            if (!DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            var newEncounterLog = new LogModel
            {
                User = gameMaster.TrainerName,
                Action = $"activated a new encounter ({encounter.Name}) at {DateTime.Now}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), newEncounterLog);
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
            if (!DatabaseUtility.UpdateEncounter(encounter))
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
            if (!DatabaseUtility.UpdateEncounter(encounter))
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

        private static EncounterParticipantModel GetWithUpdatedHP(EncounterParticipantModel participant)
        {
            return Enum.Parse<EncounterParticipantType>(participant.Type, true) switch
            {
                EncounterParticipantType.Trainer => EncounterParticipantModel.FromTrainer(participant.ParticipantId, participant.Position),
                EncounterParticipantType.Pokemon => EncounterParticipantModel.FromPokemon(participant.ParticipantId, participant.Position),
                EncounterParticipantType.Npc => EncounterParticipantModel.FromNpc(participant.ParticipantId, participant.Position),
                _ => throw new ArgumentOutOfRangeException(nameof(participant.Type)),
            };
        }

        private static void SendRepositionLog(string gameId, string participantName, MapPositionModel position)
        {
            var repositionLog = new LogModel
            {
                User = participantName,
                Action = $"moved to point ({position.X}, {position.Y}) at {DateTime.Now}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), repositionLog);
        }

        private static double GetDistance(MapPositionModel start, MapPositionModel end)
        {
            return Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
        }
    }
}
