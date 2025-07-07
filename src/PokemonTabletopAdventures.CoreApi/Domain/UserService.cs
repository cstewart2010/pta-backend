using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class UserService(ITrainerService trainerService) : AbstractMongoService<UserDto>(MongoCollection.Users), IUserService
{
    private readonly ITrainerService _trainerService = trainerService;

    public async Task DeleteUser(Guid userId)
    {
        await ThrowIfNull(
            userId,
            id => Collection.FindOneAndDelete(user => user.UserId == id),
            PropertyNames.UserId);
        
        var trainers = await _trainerService.GetAllUserTrainers(userId);
        var games = trainers.Select(trainer => trainer.GameId);
        foreach(var gameId in games)
        {
            await _trainerService.DeleteTrainer(gameId, userId);
        }
    }

    public async Task<User> GetUserById(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.Find(user => user.UserId == id).SingleOrDefault(),
            PropertyNames.UserId);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<User> GetUserByUsername(string username)
    {
        var dto = await ThrowIfNull(
            username,
            username => Collection.Find(user => user.Username == username).SingleOrDefault(),
            PropertyNames.Username);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        var dtos = await Task.FromResult(Collection.Find(user => true).ToEnumerable());
        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task<IEnumerable<User>> GetUsers(int offset, int limit)
    {
        var dtos = await Task.FromResult(Collection.Find(user => true).Skip(offset).Limit(limit).ToEnumerable());
        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task PostUser(User user, string passwordHash)
    {
        var dto = DtoHandler.ParseFromModel(user);
        dto.PasswordHash = passwordHash;
        dto.IsOnline = true;
        await PostDocument(dto);
    }

    public async Task<User> UpdateUser(User updatedUser)
    {
        var currentUser = Collection.Find(user => user.UserId == updatedUser.UserId).SingleOrDefault();
        var dto = DtoHandler.ParseFromModel(updatedUser);
        dto.PasswordHash = currentUser.PasswordHash;
        dto.IsOnline = currentUser.IsOnline;
        await UpsertDocument(
            Builders<UserDto>.Filter.Eq(user => user.UserId, updatedUser.UserId),
            dto);

        return await GetUserById(dto.UserId);
    }

    public async Task<User> UpdateUserActivityToken(Guid userId, string token)
    {
        var dto = await UpdateDocument(
            userId,
            user => user.UserId == userId,
            Builders<UserDto>.Update.Set(PropertyNames.ActivityToken, token));

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<User> UpdateUserOnlineStatus(Guid userId, bool isOnline)
    {
        var dto = await UpdateDocument(
            userId,
            user => user.UserId == userId,
            UserStatusUpdate(isOnline));

        return DtoHandler.ParseFromDto(dto);
    }

    private static UpdateDefinition<UserDto> UserStatusUpdate(bool isOnline)
    {
        if (isOnline)
        {
            return Builders<UserDto>.Update.Set(PropertyNames.IsOnline, isOnline);
        }

        return Builders<UserDto>.Update.Combine(
            Builders<UserDto>.Update.Set(PropertyNames.IsOnline, isOnline),
            Builders<UserDto>.Update.Set(PropertyNames.ActivityToken, string.Empty));
    }
}
