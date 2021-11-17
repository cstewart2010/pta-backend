using Microsoft.AspNetCore.Http;
using System;
using TheReplacements.PTA.Common.Utilities;

namespace TheReplacements.PTA.Services.Core
{
    public static class Header
    {
        private static readonly string CookieKey = Environment.GetEnvironmentVariable("CookieKey");
        public static string AccessUrl => "*";
        public static string GetCookie() => DatabaseUtility.HashPassword(CookieKey);
        public static bool VerifyCookies(IRequestCookieCollection cookies) => cookies.TryGetValue("ptaSessionAuth", out var cookie) && DatabaseUtility.VerifyTrainerPassword(CookieKey, cookie);
    }
}
