using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Trainers;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Extensions;

internal static class RequestExtensions
{
    static RequestExtensions()
    {
        AuthKey = Environment.GetEnvironmentVariable(HeaderNames.CookieKey, EnvironmentVariableTarget.Process)!;
    }

    internal static string AuthKey { get; }

    public static void IsUserGM(
        this HttpRequest request,
        IEncryptionService encryptionService,
        string sessionAuth,
        User user,
        Trainer gameMaster)
    {
        var isAdmin = user.SiteRole == UserRoleOnSite.SiteAdmin;
        if (gameMaster.IsGM || isAdmin)
        {
            request.VerifyIdentity(user, encryptionService, sessionAuth);
        }

        throw new PtaUnauthorizedException($"User {gameMaster.TrainerId} is not a GM or Admin");
    }

    public static void VerifyIdentity(
        this HttpRequest request,
        User user,
        IEncryptionService encryptionService,
        string sessionAuth)
    {
        encryptionService.VerifySecret($"{AuthKey}_{user.UserId}", sessionAuth);
    }
}
