namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IEncryptionService
{
    /// <summary>
    /// Generates a time-based access token for checking against idle users
    /// </summary>
    public Task<string> GenerateToken(DateTime generationTime);

    /// <summary>
    /// Encrypts a password for storage
    /// </summary>
    /// <param name="secret">The secret to hash</param>
    public Task<string> HashSecret(string secret);

    /// <summary>
    /// Verifys that the secret matches the encryption
    /// </summary>
    /// <param name="secret">The secret to validate</param>
    /// <param name="hashedSecret">The hashed form of the correct secret</param>
    public Task VerifySecret(
        string secret,
        Guid gameId);

    public Task VerifySecret(
        string secret,
        string username);
}
