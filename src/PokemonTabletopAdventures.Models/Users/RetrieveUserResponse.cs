namespace PokemonTabletopAdventures.Models.Users;

public class RetrieveUserResponse
{
    public required ICollection<User> Users { get; set; }
}