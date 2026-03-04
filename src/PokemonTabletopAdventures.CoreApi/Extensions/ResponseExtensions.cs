using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;

namespace PokemonTabletopAdventures.CoreApi.Extensions;

internal static class ResponseExtensions
{
    public static async Task AssignAuthAndToken(
        this HttpResponse response,
        IEncryptionService encryptionService,
        Guid trainerId)
    {
        var authHash = await encryptionService.HashSecret($"{RequestExtensions.AuthKey}_{trainerId}");
        response.Headers.Append(HeaderNames.SessionAuth, authHash);
    }
}
