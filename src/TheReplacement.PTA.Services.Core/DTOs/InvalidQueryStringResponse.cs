using System.Collections.Generic;

namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class InvalidQueryStringResponse : AbstractDto
{
    public InvalidQueryStringResponse()
    {
        Message = "Missing the following parameters in the query";
    }

    public required IEnumerable<string> MissingParameters { get; set; }
    public required IEnumerable<string> InvalidParameters { get; set; }
}
