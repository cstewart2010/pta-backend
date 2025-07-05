using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class UserService(ITrainerService trainerService) : AbstractService<UserModel>(MongoCollection.UserMessageThreads), IUserService
{
    private readonly ITrainerService _trainerService = trainerService;

    public async Task DeleteUser(Guid userId)
    {
        await ThrowIfNull(
            userId,
            id => Collection.FindOneAndDelete(user => user.UserId == id),
            "UserId");
        
        var trainers = await _trainerService.GetAllUserTrainers(userId);
        var games = trainers.Select(trainer => trainer.GameId);
        foreach(var gameId in games)
        {
            await _trainerService.DeleteTrainer(gameId, userId);
        }
    }

    public async Task<UserModel> GetUserById(Guid id)
    {
        return await ThrowIfNull(
            id,
            id => Collection.Find(user => user.UserId == id).SingleOrDefault(),
            "UserId");
    }

    public async Task<UserModel> GetUserByUsername(string username)
    {
        return await ThrowIfNull(
            username,
            username => Collection.Find(user => user.Username == username).SingleOrDefault(),
            "Username");
    }

    public async Task<IEnumerable<UserModel>> GetUsers()
    {
        return await Task.FromResult(Collection.Find(user => true).ToEnumerable());
    }

    public async Task<IEnumerable<UserModel>> GetUsers(int offset, int limit)
    {
        return await Task.FromResult(Collection.Find(user => true).Skip(offset).Limit(limit).ToEnumerable());
    }

    public async Task PostUser(UserModel user)
    {
        await PostDocument(user);
    }

    public async Task<UserModel> UpdateUser(UserModel updatedUser)
    {
        await UpsertDocument(
            Builders<UserModel>.Filter.Eq(user => user.UserId, updatedUser.UserId),
            updatedUser);

        return updatedUser;
    }

    public async Task<UserModel> UpdateUserActivityToken(Guid userId, string token)
    {
        return await UpdateDocument(
            userId,
            user => user.UserId == userId,
            Builders<UserModel>.Update.Set("ActivityToken", token));
    }

    public async Task<UserModel> UpdateUserOnlineStatus(Guid userId, bool isOnline)
    {
        return await UpdateDocument(
            userId,
            user => user.UserId == userId,
            UserStatusUpdate(isOnline));
    }

    private static UpdateDefinition<UserModel> UserStatusUpdate(bool isOnline)
    {
        if (isOnline)
        {
            return Builders<UserModel>.Update.Set("IsOnline", isOnline);
        }

        return Builders<UserModel>.Update.Combine(
            Builders<UserModel>.Update.Set("IsOnline", isOnline),
            Builders<UserModel>.Update.Set("ActivityToken", string.Empty));
    }
}
