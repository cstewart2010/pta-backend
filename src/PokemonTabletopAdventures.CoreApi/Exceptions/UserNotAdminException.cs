namespace PokemonTabletopAdventures.CoreApi.Exceptions;

public class UserNotAdminException(Guid id) : PtaUnauthorizedException($"{id} is not a Site Admin")
{
}