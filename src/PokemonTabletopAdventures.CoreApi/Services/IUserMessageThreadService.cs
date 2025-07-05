using System;
using System.Threading.Tasks;
using PokemonTabletopAdventures.Models;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IUserMessageThreadService
{
    /// <summary>
    /// Returns a message thread matching the id
    /// </summary>
    /// <param name="id">The message id</param>
    public Task<UserMessageThreadModel> GetMessageById(Guid id);

    /// <summary>
    /// Attempts to add a message thread using the provided document
    /// </summary>
    /// <param name="thread">The thread to add</param>
    public Task PostThread(UserMessageThreadModel thread);

    /// <summary>
    /// Attempts to replace the previous thread with the new data
    /// </summary>
    /// <param name="updatedThread">The updated thread data</param>
    public Task<UserMessageThreadModel> UpdateThread(UserMessageThreadModel updatedThread);
}
