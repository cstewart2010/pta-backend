using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Npcs;

public class UpdateNpcResponse
{
    public required ICollection<Npc> Npcs { get; set; }
}
