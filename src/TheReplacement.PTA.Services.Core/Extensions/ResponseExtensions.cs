using Microsoft.AspNetCore.Http;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Services.Core.Extensions
{
    internal static class ResponseExtensions
    {
        private const string AccessUrl = "*";

        public static void UpdateAccessControl(this HttpResponse response)
        {
            response.Headers["Access-Control-Allow-Origin"] = AccessUrl;
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

            response.Headers.Append("ptaSessionAuth", GetSessionAuth());
            response.Headers.Append("ptaActivityToken", token);
        }

        public static void RefreshToken(
            this HttpResponse response,
            string id)
        {
            var updatedToken = EncryptionUtility.GenerateToken();
            DatabaseUtility.UpdateTrainerActivityToken(id, updatedToken);
            response.Headers.Append("ptaActivityToken", updatedToken);
        }
        private static string GetSessionAuth()
        {
            return EncryptionUtility.HashSecret(RequestExtensions.AuthKey);
        }
    }
}
