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
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/setting")]
    public class SettingController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }
        private static readonly byte[] Buffer = new byte[36];

        public SettingController()
        {
            Collection = MongoCollection.Settings;
        }

        [HttpGet]
        public IEnumerable<string> GetEnvironments()
        {
            return Enum.GetNames<Environments>().Where(environment => environment != "Default");
        }

        [HttpGet("{gameId}")]
        public async Task<ActionResult<SettingModel>> GetActiveSetting(Guid gameId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                await StreamSetting(gameId);
            }

            return BadRequest();
        }

        [HttpGet("{gameId}/{gameMasterId}/all")]
        public ActionResult<IEnumerable<SettingModel>> GetAllSettings(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            return DatabaseUtility.FindAllSettings(gameId).ToList();
        }

        [HttpPost("{gameId}/{gameMasterId}")]
        public async Task<ActionResult> CreateSettingAsync(Guid gameId, Guid gameMasterId)
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

            var encounter = new SettingModel
            {
                SettingId = Guid.NewGuid(),
                GameId = gameId,
                Name = name,
                Type = type,
                ActiveParticipants = Array.Empty<SettingParticipantModel>(),
                Environment = Array.Empty<string>(),
                Shops = Array.Empty<Guid>()
            };

            var (addResult, error) = DatabaseUtility.TryAddSetting(encounter);
            if (!addResult)
            {
                return BadRequest(error);
            }

            return Ok();
        }

        [HttpPut("{gameId}/{trainerId}")]
        public async Task<ActionResult> AddToActiveSettingAsync(Guid gameId, Guid trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindActiveSetting(gameId);
            if (encounter == null)
            {
                return NotFound();
            }

            var json = await Request.GetRequestBody();
            var participant = json.ToObject<SettingParticipantModel>();
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
            if (!DatabaseUtility.UpdateSetting(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPut("{gameId}/{gameMasterId}/{participantId}/remove")]
        public ActionResult RemoveFromActiveSetting(Guid gameId, Guid gameMasterId, Guid participantId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            return RemoveFromParticipants(gameId, participantId);
        }


        [HttpPut("{gameId}/{trainerId}/{pokemonId}/return")]
        public ActionResult ReturnToPokeball(Guid gameId, Guid trainerId, Guid pokemonId)
        {
            if (!Request.VerifyIdentity(trainerId)) 
            { 
                return Unauthorized();
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon.TrainerId != trainerId)
            {
                return BadRequest();
            }

            return RemoveFromParticipants(gameId, pokemonId);
        }

        [HttpPut("{gameId}/{trainerId}/{pokemonId}/catch")]
        public ActionResult CatchPokemon(Guid gameId, Guid trainerId, Guid pokemonId, [FromQuery] int catchRate, [FromQuery] string pokeball, [FromQuery] string nickname)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized(trainerId);
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon.TrainerId != Guid.Empty)
            {
                return BadRequest(pokemonId);
            }

            var encounter = DatabaseUtility.FindActiveSetting(gameId);
            if (encounter == null || pokemon.GameId != gameId)
            {
                return BadRequest(gameId);
            }

            if (!Enum.TryParse<Pokeball>(pokeball, true, out var pokeballEnum))
            {
                return BadRequest(pokeball);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainer?.IsOnline != true)
            {
                return NotFound(trainerId);
            }

            var items = new[]
            {
                new ItemModel
                {
                    Name = pokeballEnum.ToString().Replace("_", " "),
                    Amount = 1
                }
            };
            RemoveItemsFromTrainer(trainer, items);

            var pokeballModifier = GetPokeballModifier(pokemon, trainerId, pokeballEnum, encounter.Environment);
            var random = new Random();
            var check = random.Next(1, 101) + pokeballModifier;
            var log = new LogModel
            {
                User = DatabaseUtility.FindTrainerById(trainerId, gameId).TrainerName,
                Action = $"failed to catch {pokemon.Nickname} at {DateTime.UtcNow}"
            };

            if (check < catchRate)
            {
                encounter.ActiveParticipants = encounter.ActiveParticipants.Where(participant => participant.ParticipantId != pokemonId);
                DatabaseUtility.UpdateSetting(encounter);
                pokemon.Pokeball = pokeballEnum.ToString().Replace("_","");
                pokemon.OriginalTrainerId = trainerId;
                pokemon.TrainerId = trainerId;
                var allMons = DatabaseUtility.FindPokemonByTrainerId(trainerId, gameId).Where(pokemon => pokemon.IsOnActiveTeam).Count();
                pokemon.IsOnActiveTeam = allMons < 6;
                if (!string.IsNullOrWhiteSpace(nickname))
                {
                    pokemon.Nickname = nickname;
                }
                DatabaseUtility.UpdatePokemon(pokemon);
                log.Action = $"successfully caught a {pokemon.SpeciesName} named '{pokemon.Nickname}' at {DateTime.UtcNow}";
            }

            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), log);
            return Ok();
        }

        [HttpPut("{gameId}/{gameMasterId}/{participantId}/position")]
        public async Task<ActionResult> UpdatePositionAsync(Guid gameId, Guid gameMasterId, Guid participantId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindActiveSetting(gameId);
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

            if (!DatabaseUtility.UpdateSetting(encounter))
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

            var encounter = DatabaseUtility.FindActiveSetting(gameId);
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

            if (!DatabaseUtility.UpdateSetting(encounter))
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
            var encounter = DatabaseUtility.FindActiveSetting(gameId);
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

            if (!DatabaseUtility.UpdateSetting(encounter))
            {
                return BadRequest();
            }

            SendRepositionLog(trainer.GameId, participant.Name, position);
            return Ok();
        }

        [HttpPut("{gameId}/{gameMasterId}/{encounterId}/active")]
        public ActionResult SetSettingToActive(Guid gameId, Guid gameMasterId, Guid encounterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            if (DatabaseUtility.FindActiveSetting(gameId) != null)
            {
                return Conflict();
            }

            var encounter = DatabaseUtility.FindSetting(encounterId);
            if (encounter == null)
            {
                return NotFound(encounterId);
            }

            encounter.IsActive = true;
            if (!DatabaseUtility.UpdateSetting(encounter))
            {
                return BadRequest();
            }

            var newSettingLog = new LogModel
            {
                User = gameMaster.TrainerName,
                Action = $"activated a new encounter ({encounter.Name}) at {DateTime.Now}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), newSettingLog);
            return Ok();
        }

        [HttpPut("{gameId}/{gameMasterId}/{encounterId}/inactive")]
        public ActionResult SetSettingToInactive(Guid gameId, Guid gameMasterId, Guid encounterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var encounter = DatabaseUtility.FindSetting(encounterId);
            if (encounter == null)
            {
                return NotFound(encounterId);
            }

            encounter.IsActive = false;
            if (!DatabaseUtility.UpdateSetting(encounter))
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

            var encounter = DatabaseUtility.FindSetting(encounterId);
            if (encounter == null)
            {
                return NotFound(encounterId);
            }

            encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant => GetWithUpdatedHP(participant, gameId));
            if (!DatabaseUtility.UpdateSetting(encounter))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{gameId}/{gameMasterId}/{encounterId}")]
        public ActionResult DeleteSetting(Guid gameId, Guid gameMasterId, Guid encounterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteSetting(encounterId))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete("{gameId}/{gameMasterId}")]
        public ActionResult DeleteSettings(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteSettingsByGameId(gameId))
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
            if (!Enum.TryParse<SettingType>(type, true, out _))
            {
                return (null, null, new GenericMessage($"Make sure the type parameter is one of {string.Join(',', Enum.GetNames(typeof(SettingType)))}"));
            }

            return (name, type, null);
        }

        private async Task StreamSetting(Guid gameId)
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

        private ActionResult RemoveFromParticipants(Guid gameId, Guid participantId)
        {
            var encounter = DatabaseUtility.FindActiveSetting(gameId);
            var removedParticipant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == participantId);
            encounter.ActiveParticipants = encounter.ActiveParticipants.Where(participant => participant.ParticipantId != participantId);

            if (!DatabaseUtility.UpdateSetting(encounter))
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
            var encounter = DatabaseUtility.FindActiveSetting(gameId);
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

        private static int GetPokeballModifier(PokemonModel pokemon, Guid trainerId, Pokeball pokeball, string[] environments)
        {
            if (pokemon.CurrentHP < 1 && pokeball != Pokeball.Save_Ball)
            {
                return 100;
            }
            var consideredBasic = 5;
            var consideredGreat = 0;
            var consideredUltra = -5;
            var environment = Environments.Default;
            var types = PokemonTypes.None;
            foreach (var type in pokemon.Type.Split('/'))
            {
                types |= Enum.Parse<PokemonTypes>(type, true);
            }
            if (environments != null)
            {
                foreach (var env in environments)
                {
                    environment |= Enum.Parse<Environments>(env, true);
                }
            }

            return pokeball switch
            {
                Pokeball.Park_Ball => environment.HasFlag(Environments.Safari) ? -20 : consideredBasic,
                Pokeball.Cherish_Ball => consideredUltra,
                Pokeball.Premier_Ball => consideredUltra,
                Pokeball.Sport_Ball => environment.HasFlag(Environments.Safari) ? -20 : consideredBasic,
                Pokeball.Heavy_Ball => Enum.TryParse<Weight>(pokemon.Weight, true, out var result) && (result == Weight.Heavy || result == Weight.Superweight) ? -15 : consideredBasic,
                Pokeball.Level_Ball => consideredBasic,
                Pokeball.Nest_Ball => consideredBasic,
                Pokeball.Rainforest_Ball => environment.HasFlag(Environments.Rainforest) ? -12 : consideredBasic,
                Pokeball.Great_Ball => consideredGreat,
                Pokeball.Safari_Ball => environment.HasFlag(Environments.Safari) ? -20 : consideredBasic,
                Pokeball.Luxury_Ball => consideredUltra,
                Pokeball.Lure_Ball => environment.HasFlag(Environments.InCombat) ? -10 : consideredBasic,
                Pokeball.Heat_Ball => types.HasFlag(PokemonTypes.Electric) || types.HasFlag(PokemonTypes.Fire) ? -15 : consideredBasic,
                Pokeball.Cave_Ball => environment.HasFlag(Environments.Cave) ? -12 : consideredBasic,
                Pokeball.Earth_Ball => types.HasFlag(PokemonTypes.Grass) || types.HasFlag(PokemonTypes.Ground) ? -15 : consideredBasic,
                Pokeball.Fine_Ball => types.HasFlag(PokemonTypes.Normal) || types.HasFlag(PokemonTypes.Fairy) ? -15 : consideredBasic,
                Pokeball.Taiga_Ball => environment.HasFlag(Environments.Taiga) ? -12 : consideredBasic,
                Pokeball.Save_Ball => pokemon.CurrentHP < 1 ? -10 : consideredBasic,
                Pokeball.Artic_Ball => environment.HasFlag(Environments.Artic) ? -12 : consideredBasic,
                Pokeball.Desert_Ball => environment.HasFlag(Environments.Desert) ? -12 : consideredBasic,
                Pokeball.Haunt_Ball => types.HasFlag(PokemonTypes.Dark) || types.HasFlag(PokemonTypes.Ghost) ? -15 : consideredBasic,
                Pokeball.Urban_Ball => environment.HasFlag(Environments.Urban) ? -12 : consideredBasic,
                Pokeball.Net_Ball => types.HasFlag(PokemonTypes.Water) || types.HasFlag(PokemonTypes.Bug) ? -15 : consideredBasic,
                Pokeball.Freshwater_Ball => environment.HasFlag(Environments.Freshwater) ? -12 : consideredBasic,
                Pokeball.Beach_Ball => environment.HasFlag(Environments.Beach) ? -12 : consideredBasic,
                Pokeball.Timer_Ball => consideredUltra,
                Pokeball.Mystic_Ball => types.HasFlag(PokemonTypes.Dragon) || types.HasFlag(PokemonTypes.Psychic) ? -15 : consideredBasic,
                Pokeball.Air_Ball => types.HasFlag(PokemonTypes.Flying) || types.HasFlag(PokemonTypes.Ice) ? -15 : consideredBasic,
                Pokeball.Fast_Ball => consideredUltra,
                Pokeball.Basic_Ball => consideredUltra,
                Pokeball.Heal_Ball => consideredGreat,
                Pokeball.Master_Ball => -100,
                Pokeball.Tundra_Ball => environment.HasFlag(Environments.Tundra) ? -12 : consideredBasic,
                Pokeball.Friend_Ball => consideredGreat,
                Pokeball.Grassland_Ball => environment.HasFlag(Environments.Grassland) ? -12 : consideredBasic,
                Pokeball.Marsh_Ball => environment.HasFlag(Environments.Marsh) ? -12 : consideredBasic,
                Pokeball.Quick_Ball => consideredBasic,
                Pokeball.Repeat_Ball => DatabaseUtility.GetPokedexItem(trainerId, pokemon.DexNo)?.IsCaught == true ? -10 : consideredBasic,
                Pokeball.Dream_Ball => Enum.TryParse<Status>(pokemon.PokemonStatus, true, out var result) && result == Status.Asleep ? -10 : consideredBasic,
                Pokeball.Moon_Ball => consideredBasic,
                Pokeball.Dusk_Ball => environment.HasFlag(Environments.NoSunlight) ? -12 : consideredBasic,
                Pokeball.Mold_Ball => types.HasFlag(PokemonTypes.Poison) || types.HasFlag(PokemonTypes.Fighting) ? -15 : consideredBasic,
                Pokeball.Solid_Ball => types.HasFlag(PokemonTypes.Rock) || types.HasFlag(PokemonTypes.Steel) ? -15 : consideredBasic,
                Pokeball.Forest_Ball => environment.HasFlag(Environments.Forest) ? -12 : consideredBasic,
                Pokeball.Love_Ball => consideredBasic,
                Pokeball.Mountain_Ball => environment.HasFlag(Environments.Mountain) ? -12 : consideredBasic,
                _ => 1000
            };
        }

        private static SettingParticipantModel GetWithUpdatedHP(SettingParticipantModel participant, Guid gameId)
        {
            var type = Enum.Parse<SettingParticipantType>(participant.Type, true);
            return type switch
            {
                SettingParticipantType.Trainer => SettingParticipantModel.FromTrainer(participant.ParticipantId, gameId, participant.Position),
                SettingParticipantType.Pokemon => SettingParticipantModel.FromPokemon(participant.ParticipantId, participant.Position, type),
                SettingParticipantType.EnemyNpc => SettingParticipantModel.FromNpc(participant.ParticipantId, participant.Position, type),
                SettingParticipantType.EnemyPokemon => SettingParticipantModel.FromPokemon(participant.ParticipantId, participant.Position, type),
                SettingParticipantType.NeutralNpc => SettingParticipantModel.FromNpc(participant.ParticipantId, participant.Position, type),
                SettingParticipantType.NeutralPokemon => SettingParticipantModel.FromPokemon(participant.ParticipantId, participant.Position, type),
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
