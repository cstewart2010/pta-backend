using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheReplacements.PTA.Common.Utilities;

namespace TheReplacements.PTA.Services.Core.Extensions
{
    internal static class ResponseExtensions
    {
        public static void UpdateAccessControl(this HttpResponse response)
        {
            response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
        }

        public static void AssignAuthAndToken(this HttpResponse response)
        {
            response.Cookies.Append("ptaSessionAuth", Header.GetCookie());
            response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
        }

        public static void RefreshToken(this HttpResponse response)
        {
            response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
        }
    }
}
