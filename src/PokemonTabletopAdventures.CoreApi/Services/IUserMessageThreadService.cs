using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IUserMessageThreadService
{
    /// <summary>
    /// Returns a message thread matching the id
    /// </summary>
    /// <param name="id">The message id</param>
    public Task<UserMessageThread> GetMessageById(Guid id);

    /// <summary>
    /// Attempts to add a message thread using the provided document
    /// </summary>
    /// <param name="thread">The thread to add</param>
    public Task PostThread(UserMessageThread thread);

    /// <summary>
    /// Attempts to replace the previous thread with the new data
    /// </summary>
    /// <param name="updatedThread">The updated thread data</param>
    public Task<UserMessageThread> UpdateThread(UserMessageThread updatedThread);


    /// <summary>
    /// Attempts to delete a message thread matching the id
    /// </summary>
    /// <param name="id">The message id</param>
    public Task DeleteThread(Guid id);
}
