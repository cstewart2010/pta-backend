using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
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
        private static readonly byte[] Buffer = new byte[36];

        public EncounterController()
        {
            Collection = MongoCollection.Encounters;
        }

        [HttpGet("{gameId}")]
        public async Task<ActionResult<EncounterModel>> GetActiveEncounter(Guid gameId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                await Echo(gameId);
            }

            return BadRequest();
        }

        [HttpGet("{gameId}/{gameMasterId}/all")]
        public ActionResult<IEnumerable<EncounterModel>> GetAllEncounters(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            return DatabaseUtility.FindAllEncounters(gameId).ToList();
        }

        [HttpPost("{gameId}/{gameMasterId}")]
        public async Task<ActionResult> CreateEncounterAsync(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var (name, type, message) = await GetCreationParametersAsync();
            if (message != null)
            {
                return BadRequest(message);
            }

            var encounter = new EncounterModel
            {
                EncounterId = Guid.NewGuid(),
                GameId = gameId,
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

        [HttpPut("{gameId}/{trainerId}")]
        public async Task<ActionResult> AddToActiveEncounterAsync(Guid gameId, Guid trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindActiveEncounter(gameId);
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

        [HttpPut("{gameId}/{gameMasterId}/{participantId}/remove")]
        public ActionResult RemoveFromActiveEncounter(Guid gameId, Guid gameMasterId, Guid participantId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindActiveEncounter(gameId);
            var removedParticipant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == participantId);
            encounter.ActiveParticipants = encounter.ActiveParticipants.Where(participant => participant.ParticipantId != participantId);

            if (!DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            var removalLog = new LogModel
            {
                User = removedParticipant.Name,
                Action = $"has been removed from {encounter.Name} at {DateTime.Now}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), removalLog);
            return Ok();
        }

        [HttpPut("{gameId}/{gameMasterId}/{participantId}/position")]
        public async Task<ActionResult> UpdatePositionAsync(Guid gameId, Guid gameMasterId, Guid participantId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindActiveEncounter(gameId);
            if (encounter == null)
            {
                return NotFound();
            }

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

            SendRepositionLog(gameId, participant.Name, position);
            return Ok();
        }

        [HttpPut("{gameId}/{trainerId}/trainer_position")]
        public async Task<ActionResult> UpdateTrainerPositionAsync(Guid gameId, Guid trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindActiveEncounter(gameId);
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

            SendRepositionLog(gameId, participant.Name, position);
            return Ok();
        }

        [HttpPut("{gameId}/{trainerId}/{pokemonId}/pokemon_position")]
        public async Task<ActionResult> UpdateTrainerPokemonPositionAsync(Guid gameId, Guid trainerId, Guid pokemonId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            var encounter = DatabaseUtility.FindActiveEncounter(gameId);
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

        [HttpPut("{gameId}/{gameMasterId}/{encounterId}/active")]
        public ActionResult SetEncounterToActive(Guid gameId, Guid gameMasterId, Guid encounterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            if (DatabaseUtility.FindActiveEncounter(gameId) != null)
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

        [HttpPut("{gameId}/{gameMasterId}/{encounterId}/inactive")]
        public ActionResult SetEncounterToInactive(Guid gameId, Guid gameMasterId, Guid encounterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
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

        [HttpPut("{gameId}/{gameMasterId}/{encounterId}/hp")]
        public ActionResult UpdateParticipantsHp(Guid gameId, Guid gameMasterId, Guid encounterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindEncounter(encounterId);
            if (encounter == null)
            {
                return NotFound(encounterId);
            }

            encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant => GetWithUpdatedHP(participant, gameId));
            if (!DatabaseUtility.UpdateEncounter(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{gameId}/{gameMasterId}/{encounterId}")]
        public ActionResult DeleteEncounter(Guid gameId, Guid gameMasterId, Guid encounterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteEncounter(encounterId))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{gameId}/{gameMasterId}")]
        public ActionResult DeleteEncounters(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteEncountersByGameId(gameId))
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

        private async Task Echo(Guid gameId)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var recieved = await RecieveAsync(webSocket);
            while (!recieved.CloseStatus.HasValue)
            {
                await SendAsync
                (
                    webSocket,
                    gameId,
                    recieved.MessageType,
                    recieved.EndOfMessage
                );

                recieved = await RecieveAsync(webSocket);
            }

            await webSocket.CloseAsync
            (
                recieved.CloseStatus.Value,
                recieved.CloseStatusDescription,
                CancellationToken.None
            );
        }

        private static async Task<WebSocketReceiveResult> RecieveAsync(WebSocket webSocket)
        {
            return await webSocket.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None);
        }

        private static async Task SendAsync(
            WebSocket webSocket,
            Guid gameId,
            WebSocketMessageType messageType,
            bool endOfMessage)
        {
            var encounter = DatabaseUtility.FindActiveEncounter(gameId);
            var message = JsonConvert.SerializeObject(encounter);
            var messageAsBytes = System.Text.Encoding.ASCII.GetBytes(message);
            await webSocket.SendAsync
            (
                new ArraySegment<byte>(messageAsBytes),
                messageType,
                endOfMessage,
                CancellationToken.None
            );
        }

        private static EncounterParticipantModel GetWithUpdatedHP(EncounterParticipantModel participant, Guid gameId)
        {
            var type = Enum.Parse<EncounterParticipantType>(participant.Type, true);
            return type switch
            {
                EncounterParticipantType.Trainer => EncounterParticipantModel.FromTrainer(participant.ParticipantId, gameId, participant.Position),
                EncounterParticipantType.Pokemon => EncounterParticipantModel.FromPokemon(participant.ParticipantId, participant.Position, type),
                EncounterParticipantType.EnemyNpc => EncounterParticipantModel.FromNpc(participant.ParticipantId, participant.Position, type),
                EncounterParticipantType.EnemyPokemon => EncounterParticipantModel.FromPokemon(participant.ParticipantId, participant.Position, type),
                EncounterParticipantType.NeutralNpc => EncounterParticipantModel.FromNpc(participant.ParticipantId, participant.Position, type),
                EncounterParticipantType.NeutralPokemon => EncounterParticipantModel.FromPokemon(participant.ParticipantId, participant.Position, type),
                _ => throw new ArgumentOutOfRangeException(nameof(participant.Type)),
            };
        }

        private static void SendRepositionLog(Guid gameId, string participantName, MapPositionModel position)
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
