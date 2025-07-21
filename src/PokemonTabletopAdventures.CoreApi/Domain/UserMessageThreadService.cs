using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class UserMessageThreadService(
    IRepositoryService repositoryService,
    IUserService userService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper) : AbstractMongoService<UserMessageThreadDto>(repositoryService, MongoCollection.UserMessageThreads), IUserMessageThreadService
{
    private readonly IUserService _userService = userService;
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;

    public async Task DeleteThread(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.DeleteAsync(message => message.MessageId == id),
            PropertyNames.MessageId);

        var ids = dto.Messages.Select(x => x.User).Distinct();

        foreach (var userId in ids)
        {
            try
            {
                var user = await _userService.GetUserById(userId);
                user.Messages.Remove(id);
                await _userService.UpdateUser(user);
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
            id => Collection.GetOneAsync(message => message.MessageId == id),
            PropertyNames.MessageId);

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task PostThread(UserMessageThread thread)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(thread);
        await PostUniqueDocument(dto, x => x.MessageId == thread.MessageId);
    }

    public async Task<UserMessageThread> UpdateThread(UserMessageThread updatedThread)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(updatedThread);
        await UpsertDocument(
            thread => thread.MessageId,
            updatedThread.MessageId,
            dto);

        return await GetMessageById(updatedThread.MessageId);
    }
}
