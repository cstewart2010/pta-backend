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
using TheReplacement.PTA.Services.Core.Objects;

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

        [HttpGet("{gameId}/{gameMasterId}/{npcId}")]
        public ActionResult<PublicNpc> GetNpc(string gameId, string gameMasterId, string npcId)
        {
            if (!Request.VerifyIdentity(gameMasterId))
            {
                return Unauthorized();
            }

            var npc = DatabaseUtility.FindNpc(npcId);
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            if (npc == null)
            {
                return NotFound(npcId);
            }

            if (gameMaster.GameId != npc.GameId)
            {
                return Conflict();
            }

            return new PublicNpc(npc);
        }

        [HttpGet("{gameId}/{gameMasterId}/{npcId}/{pokemonId}")]
        public ActionResult<PokemonModel> GetNpcMon(string gameMasterId, string gameId, string npcId, string pokemonId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var pokemon = DatabaseUtility.FindPokemonByTrainerId(npcId).SingleOrDefault(pokemon => pokemon.PokemonId == pokemonId);
            if (pokemon == null)
            {
                return NotFound(pokemonId);
            }

            return pokemon;

        }

        [HttpGet("{gameId}/{gameMasterId}/npcs/all")]
        public ActionResult<IEnumerable<PublicNpc>> GetNpcsInGame(string gameMasterId, string gameId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var npcs = DatabaseUtility.FindNpcsByGameId(gameId);
            return npcs.Select(npc => new PublicNpc(npc)).ToList();
        }

        [HttpPost("{gameId}/{gameMasterId}/new")]
        public async Task<ActionResult<NpcModel>> CreateNewNpcAsync(string gameMasterId, string gameId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            var npc = await CreateNpcAsync();
            npc.GameId = gameMaster.GameId;
            if (!DatabaseUtility.TryAddNpc(npc, out var error))
            {
                return BadRequest(error);
            }

            Response.RefreshToken(gameMasterId);
            return npc;
        }

        [HttpPost("{gameId}/{gameMasterId}/{npcId}/new")]
        public async Task<ActionResult> CreateNewNpcMonAsync(string gameMasterId, string gameId, string npcId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var npc = DatabaseUtility.FindNpc(npcId);
            if (npc == null)
            {
                return BadRequest(npcId);
            }

            var newPokemon = (await Request.GetRequestBody()).ToObject<IEnumerable<NewPokemon>>();
            AddNpcPokemon(newPokemon, npcId, gameId);
            return Ok();
        }

        [HttpPut("{gameId}/{gameMasterId}/{npcId}/addStats")]
        public async Task<ActionResult<PublicNpc>> AddNpcStats(string gameMasterId,  string npcId, string gameId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId)) 
            { 
                return Unauthorized(); 
            }

            var result = await Request.TryCompleteNpc();
            if (!result)
            {
                return BadRequest(new GenericMessage("Failed to update Npc"));
            }

            var npc = DatabaseUtility.FindNpc(npcId);
            return new PublicNpc(npc);
        }

        [HttpDelete("{gameId}/{gameMasterId}/{npcId}")]
        public ActionResult DeleteNpc(string gameMasterId, string gameId, string npcId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var npc = DatabaseUtility.FindNpc(npcId);
            if (npc == null)
            {
                return NotFound(npcId);
            }
           
            if (gameId != npc.GameId)
            {
                return Conflict();
            }

            if (!DatabaseUtility.DeleteNpc(npcId))
            {
                return BadRequest(npcId);
            }

            return Ok();
        }

        [HttpDelete("{gameId}/{gameMasterId}/npcs/all")]
        public ActionResult DeleteNpcsInGame(string gameMasterId, string gameId) 
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeleteNpcByGameId(gameId))
            {
                return BadRequest();
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

            // add gameMaster's GameId to npc
            return new NpcModel
            {
                NPCId = Guid.NewGuid().ToString(),
                Feats = feats,
                TrainerClasses = classes,
                TrainerName = trainerName,
                TrainerStats = new StatsModel(),
                CurrentHP = 0,
                Sprite = "acetrainer"
            };
        }

        private static void AddNpcPokemon(IEnumerable<NewPokemon> pokemon, string npcId, string gameId)
        {
            foreach (var data in pokemon.Where(data => data != null))
            {
                var nickname = data.Nickname.Length > 18 ? data.Nickname.Substring(0, 18) : data.Nickname;
                var pokemonModel = DexUtility.GetNewPokemon(data.SpeciesName, nickname, data.Form);
                pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
                pokemonModel.OriginalTrainerId = npcId;
                pokemonModel.TrainerId = npcId;
                pokemonModel.GameId = gameId;
                DatabaseUtility.TryAddPokemon(pokemonModel, out _);
            }
        }
    }
}
