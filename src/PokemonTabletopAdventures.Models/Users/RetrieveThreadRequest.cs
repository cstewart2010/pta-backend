using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Users;

public class RetrieveThreadRequest
{
    [Required]
    public required Guid UserId { get; set; }
    [Required]
    public required Guid MessageId { get; set; }
}