using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;

namespace PokemonTabletopAdventures.CoreApi.Extensions;

internal static class ResponseExtensions
{
    public static async Task AssignAuthAndToken(
        this HttpResponse response,
        IEncryptionService encryptionService,
        IUserService userService,
        Guid trainerId)
    {
        var token = await encryptionService.GenerateToken(DateTime.UtcNow);
        await userService.UpdateUserActivityToken(trainerId, token);
        var authHash = await encryptionService.HashSecret(RequestExtensions.AuthKey); ;
        response.Headers.Append(HeaderNames.SessionAuth, authHash);
        response.Headers.Append(HeaderNames.AccessToken, token);
    }

    public static async Task RefreshToken(
        this HttpResponse response,
        IEncryptionService encryptionService,
        IUserService userService,
        Guid id)
    {
        var user = await userService.GetUserById(id);
        
        var updatedToken = await encryptionService.GenerateToken(DateTime.UtcNow);
        await userService.UpdateUserActivityToken(id, updatedToken);
        response.Headers.Append(HeaderNames.AccessToken, updatedToken);
    }
}
