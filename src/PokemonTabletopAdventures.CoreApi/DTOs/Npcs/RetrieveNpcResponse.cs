using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Npcs;

public class RetrieveNpcResponse
{
    public required ICollection<Npc> Npcs { get; set; }
}
