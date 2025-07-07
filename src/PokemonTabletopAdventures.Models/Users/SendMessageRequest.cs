using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.Models.Users;

public class SendMessageRequest
{
    [Required, MinLength(1), MaxLength(255)]
    public required string MessageContent { get; set; }
    [Required]
    public required Guid UserId { get; set; }
    public Guid RecipientId { get; set; }
    public Guid MessageId { get; set; }
}
