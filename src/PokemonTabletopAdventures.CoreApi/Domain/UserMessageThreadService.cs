using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class UserMessageThreadService(IRepositoryService repositoryService) : AbstractMongoService<UserMessageThreadDto>(repositoryService, MongoCollection.UserMessageThreads), IUserMessageThreadService
{
    public async Task<UserMessageThread> GetMessageById(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(message => message.MessageId == id),
            PropertyNames.MessageId);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task PostThread(UserMessageThread thread)
    {
        var dto = DtoHandler.ParseFromModel(thread);
        await PostDocument(dto);
    }

    public async Task<UserMessageThread> UpdateThread(UserMessageThread updatedThread)
    {
        var dto = DtoHandler.ParseFromModel(updatedThread);
        await UpsertDocument(
            thread => thread.MessageId,
            updatedThread.MessageId,
            dto);

        return await GetMessageById(updatedThread.MessageId);
    }
}
