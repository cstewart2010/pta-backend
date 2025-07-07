using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Users;

public class DeleteUserRequest
{
    [Required]
    public required Guid UserId { get; set; }
    public Guid AdminId { get; set; }
}