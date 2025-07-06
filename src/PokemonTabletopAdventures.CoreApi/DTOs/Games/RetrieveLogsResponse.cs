using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class RetrieveLogsResponse
{
    [Required]
    public required ICollection<IEnumerable<Log>> LogPages { get; set; } = [];
}
