using System;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class PokemonTrainerMismatchResponse : AbstractDto
{
    internal PokemonTrainerMismatchResponse(
        Guid pokemonTrainerId,
        Guid trainerId)
    {
        Message = "Invalid trainerId";
        ExpectedTrainerId = trainerId;
        PokemonTrainerId = pokemonTrainerId;
    }

    public Guid ExpectedTrainerId { get; }
    public Guid PokemonTrainerId { get; }
}
