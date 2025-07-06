using Microsoft.AspNetCore.Http;
using PokemonTabletopAdventures.CoreApi.Services;
using System;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Extensions;

internal static class ResponseExtensions
{
    public static async Task AssignAuthAndToken(
        this HttpResponse response,
        IEncryptionService encryptionService,
        IUserService userService,
        Guid trainerId)
    {
#if !DEBUG
        var token = await encryptionService.GenerateToken();
        await userService.UpdateUserActivityToken(trainerId, token);
        var authHash = await encryptionService.HashSecret(RequestExtensions.AuthKey); ;
        response.Headers.Append(HeaderNames.SessionAuth, authHash);
        response.Headers.Append(HeaderNames.AccessToken, token);
#else
        await Task.CompletedTask;
#endif
    }

    public static async Task RefreshToken(
        this HttpResponse response,
        IEncryptionService encryptionService,
        IUserService userService,
        Guid id)
    {
#if !DEBUG
        var updatedToken = await encryptionService.GenerateToken();
        await userService.UpdateUserActivityToken(id, updatedToken);
        response.Headers.Append(HeaderNames.AccessToken, updatedToken);
#else
        await Task.CompletedTask;
#endif
    }
}
