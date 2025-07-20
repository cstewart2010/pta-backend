using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class UserService(
    IRepositoryService repositoryService,
    ITrainerService trainerService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper) : AbstractMongoService<UserDto>(repositoryService, MongoCollection.Users), IUserService
{
    private readonly ITrainerService _trainerService = trainerService;
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;

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

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<User> GetUserByUsername(string username)
    {
        var dto = await ThrowIfNull(
            username,
            username => Collection.GetOneAsync(user => user.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase)),
            PropertyNames.Username);

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<IEnumerable<User>> GetUsers()
    {
        var dtos = await Collection.GetManyAsync(user => true);
        return await Task.WhenAll(dtos.Select(_dtoToModelMapper.ParseFromDto));
    }

    public async Task<IEnumerable<User>> GetUsers(int offset, int limit)
    {
        var dtos = await Collection.GetManyAsync(user => true, offset, limit);
        return await Task.WhenAll(dtos.Select(_dtoToModelMapper.ParseFromDto));
    }

    public async Task PostUser(User user, string passwordHash)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(user);
        dto.PasswordHash = passwordHash;
        dto.IsOnline = true;
        await PostUniqueDocument(dto, x => x.UserId == user.UserId);
    }

    public async Task<User> UpdateUser(User updatedUser)
    {
        var dto = (await Collection.GetOneAsync(user => user.UserId == updatedUser.UserId))!;
        dto.ActivityToken = updatedUser.ActivityToken;
        dto.Games = updatedUser.Games;
        dto.Messages = updatedUser.Messages;
        dto.SiteRole = updatedUser.SiteRole;
        dto.Username = updatedUser.Username;
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

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<User> UpdateUserOnlineStatus(Guid userId, bool isOnline)
    {
        var dto = await UpdateDocument(
            userId,
            user => user.UserId == userId,
            UserStatusUpdate(isOnline));

        return await _dtoToModelMapper.ParseFromDto(dto);
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
