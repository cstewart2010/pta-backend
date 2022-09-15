using Microsoft.AspNetCore.Http;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Services.Core.Extensions
{
    internal static class ResponseExtensions
    {
        public static void AssignAuthAndToken(
            this HttpResponse response,
            string trainerId)
        {
            var token = EncryptionUtility.GenerateToken();
            DatabaseUtility.UpdateUserActivityToken
            (
                trainerId,
                token
            );

            response.Headers.Append("pta-session-auth", GetSessionAuth());
            response.Headers.Append("pta-activity-token", token);
        }

        public static void RefreshToken(
            this HttpResponse response,
            string id)
        {
            var updatedToken = EncryptionUtility.GenerateToken();
            DatabaseUtility.UpdateUserActivityToken(id, updatedToken);
            response.Headers.Append("pta-activity-token", updatedToken);
        }

        private static string GetSessionAuth()
        {
            return EncryptionUtility.HashSecret(RequestExtensions.AuthKey);
        }
    }
}
