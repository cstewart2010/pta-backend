namespace PokemonTabletopAdventures.Models.Npcs;

public class RetrieveNpcResponse
{
    public required ICollection<Npc> Npcs { get; set; }
}
