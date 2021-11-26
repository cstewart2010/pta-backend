using Microsoft.AspNetCore.Http;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Services.Core.Extensions
{
    internal static class ResponseExtensions
    {
        public static void UpdateAccessControl(this HttpResponse response)
        {
            response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
        }

        public static void AssignAuthAndToken(
            this HttpResponse response,
            string trainerId)
        {
            var token = EncryptionUtility.GenerateToken();
            DatabaseUtility.UpdateTrainerActivityToken
            (
                trainerId,
                token
            );

            response.Cookies.Append("ptaSessionAuth", Header.GetCookie());
            response.Cookies.Append("ptaActivityToken", token);
        }

        public static void RefreshToken(this HttpResponse response)
        {
            response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
        }
    }
}
