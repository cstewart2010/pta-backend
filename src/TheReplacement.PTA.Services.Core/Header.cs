using Microsoft.AspNetCore.Http;
using System;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Services.Core
{
    public static class Header
    {
        private static readonly string CookieKey = Environment.GetEnvironmentVariable("CookieKey");
        public static string AccessUrl => "*";
        public static string GetCookie() => EncryptionUtility.HashSecret(CookieKey);
        public static bool VerifyCookies(IRequestCookieCollection cookies, string id)
        {
            var trainer = DatabaseUtility.FindTrainerById(id);

            if (!(cookies.TryGetValue("ptaActivityToken", out var accessToken)
                && trainer.ActivityToken == accessToken
                && EncryptionUtility.ValidateToken(accessToken)))
            {
                DatabaseUtility.UpdateTrainerOnlineStatus(id, false);
                return false;
            }

            if (!(cookies.TryGetValue("ptaSessionAuth", out var cookie) && EncryptionUtility.VerifySecret(CookieKey, cookie)))
            {
                DatabaseUtility.UpdateTrainerOnlineStatus(id, false);
                return false;
            }


            return true;
        }
    }
}
