using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Users;
using System.Threading;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class UserMessageThreadService : AbstractService<UserMessageThreadDto>, IUserMessageThreadService
{
    public UserMessageThreadService() : base(MongoCollection.UserMessageThreads) { }

    public async Task<UserMessageThread> GetMessageById(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.Find(message => message.MessageId == id).SingleOrDefault(),
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
            Builders<UserMessageThreadDto>.Filter.Eq(thread => thread.MessageId, updatedThread.MessageId),
            dto);

        return await GetMessageById(updatedThread.MessageId);
    }
}
