namespace PokemonTabletopAdventures.CoreApi.Exceptions
{
    public class PtaUnauthorizedException(string message) : PtaException(message, "Authentication Failed", System.Net.HttpStatusCode.Unauthorized)
    {
    }
}
