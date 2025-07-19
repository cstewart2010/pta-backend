using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class UserService(
    IRepositoryService repositoryService,
    ITrainerService trainerService) : AbstractMongoService<UserDto>(repositoryService, MongoCollection.Users), IUserService
{
    private readonly ITrainerService _trainerService = trainerService;

    public async Task DeleteUser(Guid userId)
    {
        await ThrowIfNull(
            userId,
            id => Collection.DeleteAsync(user => user.UserId == id),
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
            id => Collection.GetOneAsync(user => user.UserId == id),
            PropertyNames.UserId);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<User> GetUserByUsername(string username)
    {
        var dto = await ThrowIfNull(
            username,
            username => Collection.GetOneAsync(user => user.Username == username),
            PropertyNames.Username);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        var dtos = await Collection.GetManyAsync(user => true);
        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task<IEnumerable<User>> GetUsers(int offset, int limit)
    {
        var dtos = await Collection.GetManyAsync(user => true, offset, limit);
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
        var currentUser = await Collection.GetOneAsync(user => user.UserId == updatedUser.UserId);
        var dto = DtoHandler.ParseFromModel(updatedUser);
        dto.PasswordHash = currentUser.PasswordHash;
        dto.IsOnline = currentUser.IsOnline;
        await UpsertDocument(
            user => user.UserId,
            updatedUser.UserId,
            dto);

        return await GetUserById(dto.UserId);
    }

    public async Task<User> UpdateUserActivityToken(Guid userId, string token)
    {
        var dto = await UpdateDocument(
            userId,
            user => user.UserId == userId,
            new Models.UpdateData(PropertyNames.ActivityToken, token));

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

    private static UpdateData[] UserStatusUpdate(bool isOnline)
    {
        if (isOnline)
        {
            return [new UpdateData(PropertyNames.IsOnline, isOnline)];
        }

        return
        [
            new UpdateData(PropertyNames.IsOnline, isOnline),
            new UpdateData(PropertyNames.ActivityToken, string.Empty)
        ];
    }
}
