using Microsoft.AspNetCore.Http;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.IO;
using System.Linq;

namespace PokemonTabletopAdventures.CoreApi.Extensions;

internal static class RequestExtensions
{
    static RequestExtensions()
    {
        AuthKey = Environment.GetEnvironmentVariable("CookieKey", EnvironmentVariableTarget.Process)!;
    }

    internal static string AuthKey { get; }

    public static string? GetJsonFromRequest(this HttpRequest request)
    {
        var jsonFile = request.Form.Files.First(file => Path.GetExtension(file.FileName).Equals(".json", StringComparison.CurrentCultureIgnoreCase));
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
        UserModel user,
        TrainerModel gameMaster)
    {
        var isAdmin = user.SiteRole == UserRoleOnSite.SiteAdmin;
        if ((gameMaster?.IsGM) != true && !isAdmin)
        {
            request.VerifyIdentity(user, encryptionService, accessToken, sessionAuth);
        }
    }

    public static void VerifyIdentity(
        this HttpRequest request,
        UserModel user,
        IEncryptionService encryptionService,
        string accessToken,
        string sessionAuth)
    {
#if !DEBUG
        if (user.ActivityToken != accessToken)
        {
            throw new PtaUnauthorizedException("Activity token is incorrect");
        }

        encryptionService.ValidateToken(accessToken);
        encryptionService.VerifySecret(AuthKey, sessionAuth);
#endif
    }
}
