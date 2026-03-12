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

    public static async Task IsUserGM(
        this HttpRequest request,
        IEncryptionService encryptionService,
        string sessionAuth,
        User user,
        Trainer gameMaster)
    {
        var isAdmin = user.SiteRole == UserRoleOnSite.SiteAdmin;
        if (!gameMaster.IsGM && !isAdmin)
        {
            throw new PtaUnauthorizedException($"User {gameMaster.TrainerId} is not a GM or Admin");
        }
        
        await request.VerifyIdentity(user, encryptionService, sessionAuth);
    }

    public static async Task VerifyIdentity(
        this HttpRequest request,
        User user,
        IEncryptionService encryptionService,
        string sessionAuth)
    {
        await encryptionService.VerifySecret($"{AuthKey}_{user.UserId}", sessionAuth);
    }
}
