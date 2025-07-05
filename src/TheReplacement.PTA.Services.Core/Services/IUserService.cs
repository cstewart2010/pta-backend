using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IUserService
{
    /// <summary>
    /// Returns a user matching the trainer id
    /// </summary>
    /// <param name="id">The user id</param>
    public Task<UserModel> GetUserById(Guid id);

    /// <summary>
    /// Returns a trainer matching the trainer name and game session id
    /// </summary>
    /// <param name="username">The trainer name</param>
    public Task<UserModel> GetUserByUsername(string username);

    /// <summary>
    /// Returns all users in the database
    /// </summary>
    public Task<IEnumerable<UserModel>> GetUsers();

    /// <summary>
    /// Returns a colllection of users in the database
    /// </summary>
    public Task<IEnumerable<UserModel>> GetUsers(int offset, int limit);

    /// <summary>
    /// Attempts to add a user using the provided document
    /// </summary>
    /// <param name="user">The document to add</param>
    public Task PostUser(UserModel user);

    /// <summary>
    /// Attempts to replace the previous user with the new data
    /// </summary>
    /// <param name="updatedUser">The updated user data</param>
    public Task<UserModel> UpdateUser(UserModel updatedUser);

    /// <summary>
    /// Searches for a trainer, then updates their activity token
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="token">The new activity token</param>
    public Task<UserModel> UpdateUserActivityToken(
        Guid userId,
        string token);

    /// <summary>
    /// Searches for a trainer, then updates their online status
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="isOnline">The updated online status</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="MongoCommandException" />
    public Task<UserModel> UpdateUserOnlineStatus(
        Guid userId,
        bool isOnline);

    /// <summary>
    /// Deletes the user and everything associated with them
    /// </summary>
    /// <param name="userId">The user's user id</param>
    public Task DeleteUser(Guid userId);
}
