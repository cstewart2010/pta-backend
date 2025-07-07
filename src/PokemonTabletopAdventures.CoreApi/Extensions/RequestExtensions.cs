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

    public static string? GetJsonFromRequest(this HttpRequest request)
    {
        var jsonFile = request.Form.Files.First(file => Path.GetExtension(file.FileName).Equals(Paths.JsonExt, StringComparison.CurrentCultureIgnoreCase));
        if (jsonFile.Length > 0)
        {
            using var reader = new StreamReader(jsonFile.OpenReadStream());
            var json = reader.ReadToEnd();
            reader.Close();
            return json;
        }

        return null;
    }

    public static void IsUserGM(
        this HttpRequest request,
        IEncryptionService encryptionService,
        string accessToken,
        string sessionAuth,
        User user,
        Trainer gameMaster)
    {
        var isAdmin = user.SiteRole == UserRoleOnSite.SiteAdmin;
        if ((gameMaster.IsGM) == true || isAdmin)
        {
            request.VerifyIdentity(user, encryptionService, accessToken, sessionAuth);
        }

        throw new PtaUnauthorizedException($"User {gameMaster.TrainerId} is not a GM");
    }

    public static void VerifyIdentity(
        this HttpRequest request,
        User user,
        IEncryptionService encryptionService,
        string accessToken,
        string sessionAuth)
    {
#if !DEBUG
        if (user.ActivityToken != accessToken)
        {
            throw new Exceptions.PtaUnauthorizedException(PtaExceptionParts.ExpiredTokenMessage);
        }

        encryptionService.ValidateToken(accessToken);
        encryptionService.VerifySecret(AuthKey, sessionAuth);
#endif
    }
}
