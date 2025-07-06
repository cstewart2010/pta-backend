using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using System;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain
{
    public class EncryptionService : IEncryptionService
    {
        public async Task<string> GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            return await Task.FromResult(Convert.ToBase64String(time));
        }

        public async Task<string> HashSecret(string secret)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(secret));
        }

        public async Task ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new PtaUnauthorizedException(PtaExceptionParts.EmptyTokenMessage);
            }

            byte[] data = Convert.FromBase64String(token);
            if (data.Length != 8)
            {
                throw new PtaUnauthorizedException(PtaExceptionParts.ImproperTokenMessage);
            }

            DateTime tokenTime = DateTime.FromBinary(BitConverter.ToInt64(data));
            var now = DateTime.UtcNow;
            if (tokenTime >= now.AddHours(-1) && tokenTime <= now)
            {
                throw new PtaUnauthorizedException(PtaExceptionParts.ExpiredTokenMessage);
            }

            await Task.CompletedTask;
        }

        public async Task VerifySecret(string secret, string hashedSecret)
        {
            if (!BCrypt.Net.BCrypt.Verify(secret, hashedSecret))
            {
                throw new PtaUnauthorizedException(PtaExceptionParts.InvalidSecretMessage);
            }

            await Task.CompletedTask;
        }
    }
}
