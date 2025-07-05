using System;
using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class UpdatedNpcListResponse : AbstractDto
{
    internal UpdatedNpcListResponse(IEnumerable<Guid> npcs)
    {
        Message = "Updated npc list";
        Npcs = npcs;
    }

    public IEnumerable<Guid> Npcs { get; }
}
