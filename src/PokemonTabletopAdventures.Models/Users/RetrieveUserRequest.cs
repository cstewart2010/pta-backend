namespace PokemonTabletopAdventures.Models.Users;

public class RetrieveUserRequest
{
    public Guid UserId { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}