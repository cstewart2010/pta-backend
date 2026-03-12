using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class UserMessageThreadService(
    IRepositoryService repositoryService,
    IUserService userService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<UserMessageThreadService> logger) : AbstractMongoService<UserMessageThreadDto>(repositoryService, MongoCollection.UserMessageThreads), IUserMessageThreadService
{
    public async Task DeleteThread(Guid id)
    {
        logger.LogInformation("Removing message thread {id}", id);
        var dto = await ThrowIfNull(
            id,
            x => Collection.DeleteAsync(message => message.MessageId == x, logger),
            nameof(UserMessageThread.MessageId));

        var ids = dto.Messages.Select(x => x.User).Distinct();

        foreach (var userId in ids)
        {
            try
            {
                var user = await userService.GetUserById(userId);
                user.Messages.Remove(id);
                await userService.UpdateUser(user);
            }
            catch
            {
                // suppress error
            }
        }
    }

    public async Task<UserMessageThread> GetMessageById(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            x => Collection.GetOneAsync(message => message.MessageId == x, logger),
            nameof(UserMessageThread.MessageId));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task PostThread(UserMessageThread thread)
    {
        var dto = await modelToDtoMapper.ParseFromModel(thread);
        await PostUniqueDocument(dto, x => x.MessageId == thread.MessageId, logger);
    }

    public async Task<UserMessageThread> UpdateThread(UserMessageThread updatedThread)
    {
        var dto = await modelToDtoMapper.ParseFromModel(updatedThread);
        await UpsertDocument(
            thread => thread.MessageId,
            updatedThread.MessageId,
            dto,
            logger);

        return await GetMessageById(updatedThread.MessageId);
    }
}
