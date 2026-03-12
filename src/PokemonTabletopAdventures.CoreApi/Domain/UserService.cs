using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class UserService(
    IRepositoryService repositoryService,
    ITrainerService trainerService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<UserService> logger) : AbstractMongoService<UserDto>(repositoryService, MongoCollection.Users), IUserService
{
    public async Task DeleteUser(Guid userId)
    {
        logger.LogInformation("Removing user {userId} from database", userId);
        await ThrowIfNull(
            userId,
            id => Collection.DeleteAsync(user => user.UserId == id, logger),
            nameof(User.UserId));
        
        var trainers = await trainerService.GetAllUserTrainers(userId);
        var games = trainers.Select(trainer => trainer.GameId);
        foreach(var gameId in games)
        {
            await trainerService.DeleteTrainer(userId, gameId);
        }
    }

    public async Task<User> GetUserById(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            x => Collection.GetOneAsync(user => user.UserId == x, logger),
            nameof(User.UserId));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<User> GetUserByUsername(string username)
    {
        var dto = await ThrowIfNull(
            username,
            x => Collection.GetOneAsync(user => user.Username.Equals(x, StringComparison.CurrentCultureIgnoreCase), logger),
            nameof(User.Username));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<ICollection<User>> GetUsers()
    {
        var dtos = await Collection.GetManyAsync(user => true, logger);
        return await Task.WhenAll(dtos.Select(dtoToModelMapper.ParseFromDto));
    }

    public async Task<ICollection<User>> GetUsers(int offset, int limit)
    {
        var dtos = await Collection.GetManyAsync(user => true, offset, limit, logger);
        return await Task.WhenAll(dtos.Select(dtoToModelMapper.ParseFromDto));
    }

    public async Task PostUser(User user, string passwordHash)
    {
        var dto = await modelToDtoMapper.ParseFromModel(user);
        dto.PasswordHash = passwordHash;
        dto.IsOnline = true;
        await PostUniqueDocument(dto, x => x.UserId == user.UserId, logger);
    }

    public async Task<User> UpdateUser(User updatedUser)
    {
        var dto = (await Collection.GetOneAsync(user => user.UserId == updatedUser.UserId, logger)) ?? throw new UserNotFoundException(updatedUser.UserId);
        dto.Games = updatedUser.Games ?? dto.Games;
        dto.Messages = updatedUser.Messages ?? dto.Messages;
        dto.SiteRole = updatedUser.SiteRole;
        dto.Username = updatedUser.Username ?? dto.Username;
        await UpsertDocument(
            user => user.UserId,
            updatedUser.UserId,
            dto,
            logger);

        return await GetUserById(dto.UserId);
    }

    public async Task<User> UpdateUserActivityToken(Guid userId, string token)
    {
        var dto = await UpdateDocument(
            userId,
            user => user.UserId == userId,
            logger,
            new UpdateData(nameof(User.ActivityToken), token));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<User> UpdateUserOnlineStatus(Guid userId, bool isOnline)
    {
        var dto = await UpdateDocument(
            userId,
            user => user.UserId == userId,
            logger,
            UserStatusUpdate(isOnline));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    private static UpdateData[] UserStatusUpdate(bool isOnline)
    {
        if (isOnline)
        {
            return [new UpdateData(nameof(UserDto.IsOnline), isOnline)];
        }

        return
        [
            new UpdateData(nameof(UserDto.IsOnline), isOnline),
            new UpdateData(nameof(User.ActivityToken), string.Empty)
        ];
    }
}
