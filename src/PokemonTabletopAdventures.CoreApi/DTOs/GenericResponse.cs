namespace PokemonTabletopAdventures.CoreApi.DTOs;

public class GenericResponse : AbstractDto
{
    internal GenericResponse(string message)
    {
        Message = message;
    }
}
