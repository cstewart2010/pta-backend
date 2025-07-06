using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IEncryptionService
{
    /// <summary>
    /// Generates a time-based access token for checking against idle users
    /// </summary>
    public Task<string> GenerateToken();

    /// <summary>
    /// Encrypts a password for storage
    /// </summary>
    /// <param name="secret">The secret to hash</param>
    public Task<string> HashSecret(string secret);

    /// <summary>
    /// Returns true if the token falls withing the time span
    /// </summary>
    /// <param name="token"></param>
    public Task ValidateToken(string token);

    /// <summary>
    /// Verifys that the secret matches the encryption
    /// </summary>
    /// <param name="secret">The secret to validate</param>
    /// <param name="hashedSecret">The hashed form of the correct secret</param>
    public Task VerifySecret(
        string secret,
        string hashedSecret);
}
