using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class UserMessageThreadService : AbstractService<UserMessageThreadModel>, IUserMessageThreadService
{
    public UserMessageThreadService() : base(MongoCollection.UserMessageThreads) { }

    public async Task<UserMessageThreadModel> GetMessageById(Guid id)
    {
        return await ThrowIfNull(
            id,
            id => Collection.Find(message => message.MessageId == id).SingleOrDefault(),
            PropertyNames.MessageId);
    }

    public async Task PostThread(UserMessageThreadModel thread)
    {
        await PostDocument(thread);
    }

    public async Task<UserMessageThreadModel> UpdateThread(UserMessageThreadModel updatedThread)
    {
        await UpsertDocument(
            Builders<UserMessageThreadModel>.Filter.Eq(thread => thread.MessageId, updatedThread.MessageId),
            updatedThread);

        return updatedThread;
    }
}
