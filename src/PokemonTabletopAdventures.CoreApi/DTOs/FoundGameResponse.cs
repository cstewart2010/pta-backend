using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.DTOs
{
    public class FoundGameResponse : AbstractDto
    {
        internal static async Task<FoundGameResponse> ParseFromModel(
            IEnumerable<TrainerModel> models,
            Guid gameId,
            IPokemonService pokemonService,
            IPokedexService pokedexService,
            string nickname)
        {
            var trainers = await Task.WhenAll(models.Select(async trainer => await Trainer.ParseFromModel(trainer, pokemonService, pokedexService)));
            return new FoundGameResponse
            {
                Message = "Game was found",
                GameId = gameId,
                Trainers = trainers,
                Nickname = nickname,
            };
        }

        public required string Nickname { get; init; }
        public required Guid GameId { get; init; }
        public required IEnumerable<Trainer> Trainers { get; init; }
    }
}
