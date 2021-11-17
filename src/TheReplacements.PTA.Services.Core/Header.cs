using Microsoft.AspNetCore.Http;
using System;
using TheReplacements.PTA.Common.Utilities;

namespace TheReplacements.PTA.Services.Core
{
    public static class Header
    {
        private static readonly string CookieKey = Environment.GetEnvironmentVariable("CookieKey");
        public static string AccessUrl => "*";
        public static string GetCookie() => EncryptionUtility.HashSecret(CookieKey);
        public static bool VerifyCookies(IRequestCookieCollection cookies, string id)
        {
            if (!(cookies.TryGetValue("ptaActivityToken", out var accessToken) && EncryptionUtility.ValidateToken(accessToken)))
            {
                DatabaseUtility.UpdateTrainerOnlineStatus(id, false);
                return false;
            }

            return cookies.TryGetValue("ptaSessionAuth", out var cookie) && EncryptionUtility.VerifySecret(CookieKey, cookie);
        }
    }
}
