using System;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class UnauthorizedResponse : AbstractDto
{
    internal UnauthorizedResponse(Guid gameId)
    {
        Message = "Could not login in to game with provided password";
        GameId = gameId;
    }

    public Guid GameId { get; }
}
