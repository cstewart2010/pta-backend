using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class EncryptionService(IRepositoryService repositoryService) : IEncryptionService
{
    private readonly IRepositoryService _repositoryService = repositoryService;
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

    public async Task VerifySecret(string secret, Guid gameId)
    {
        var collection = _repositoryService.GetCollection<GameDto>(MongoCollection.Games);
        var game = await collection.GetOneAsync(x => x.GameId == gameId) ?? throw new UnknownEntityException<GameDto>(PropertyNames.GameId, gameId);
        if (!BCrypt.Net.BCrypt.Verify(secret, game.PasswordHash))
        {
            throw new PtaUnauthorizedException(PtaExceptionParts.InvalidSecretMessage);
        }
    }

    public async Task VerifySecret(string secret, string username)
    {
        var collection = _repositoryService.GetCollection<UserDto>(MongoCollection.Users);
        var user = await collection.GetOneAsync(x => x.Username ==  username) ?? throw new PtaUnauthorizedException(PtaExceptionParts.NoUserFoundMessage);
        if (!BCrypt.Net.BCrypt.Verify(secret, user.PasswordHash))
        {
            throw new PtaUnauthorizedException(PtaExceptionParts.InvalidSecretMessage);
        }
    }
}
