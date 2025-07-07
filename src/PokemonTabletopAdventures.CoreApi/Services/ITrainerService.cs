using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface ITrainerService
{
    /// <summary>
    /// Search for the trainer and returns them if the trainer has not completed the new user flow
    /// </summary>
    /// <param name="id">The id of the trainer to search for</param>
    public Task<Trainer> GetIncompleteTrainerById(Guid id, Guid gameId);

    /// <summary>
    /// Returns a trainer matching the trainer id
    /// </summary>
    /// <param name="id">The trainer id</param>
    /// <param name="gameId">The game session id</param>
    public Task<Trainer> GetTrainerById(Guid id, Guid gameId);

    /// <summary>
    /// Returns all trainers matching the user Id
    /// </summary>
    /// <param name="gameId">The game session id</param>
    public Task<IEnumerable<Trainer>> GetAllUserTrainers(Guid userId);

    /// <summary>
    /// Returns all trainers matching the game session id
    /// </summary>
    /// <param name="gameId">The game session id</param>
    public Task<IEnumerable<Trainer>> GetTrainersByGameId(Guid gameId);

    /// <summary>
    /// Returns a trainer matching the trainer name and game session id
    /// </summary>
    /// <param name="username">The trainer name</param>
    /// <param name="gameId">The game session id</param>
    public Task<Trainer> GetTrainerByUsername(
        string username,
        Guid gameId);

    /// <summary>
    /// Attempts to add a trainer using the provided document
    /// </summary>
    /// <param name="trainer">The document to add</param>
    public Task PostTrainer(Trainer trainer);

    /// <summary>
    /// Attempts to update the trainer with their appropriate starting stats
    /// </summary>
    /// <param name="trainerId">The id of the trainer being updated</param>
    /// <param name="origin">The trianer's origin</param>
    /// <param name="trainerClass">The trainer's stats class</param>
    /// <param name="feats">The trainer's starting feats</param>
    /// <param name="stats">The trainer's starting stats</param>
    /// <returns>True if successful</returns>
    public Task<Trainer> CompleteTrainer(
        Guid trainerId,
        string origin,
        string trainerClass,
        IEnumerable<string> feats,
        Stats stats);

    /// <summary>
    /// Attempts to replace the previous trainer with the new data
    /// </summary>
    /// <param name="updatedTrainer">The updated trainer data</param>
    public Task<Trainer> UpdateTrainer(Trainer updatedTrainer);

    /// <summary>
    /// Searches for a trainer, then updates their honors
    /// </summary>
    /// <param name="trainerId">The trainer id</param>
    /// <param name="honors">The trainer's honors</param>
    public Task<Trainer> UpdateTrainerHonors(
        Guid trainerId,
        Guid gameId,
        IEnumerable<string> honors);

    /// <summary>
    /// Searches for a trainer, then updates their item list
    /// </summary>
    /// <param name="trainerId">The trainer id</param>
    /// <param name="gameId">The game session id</param>
    /// <param name="itemList">The updated item list</param>
    /// <exception cref="ArgumentNullException" />
    public Task<Trainer> UpdateTrainerItemList(
        Guid trainerId,
        Guid gameId,
        IEnumerable<Item> itemList);

    /// <summary>
    /// Searches for a trainer, then updates their online status
    /// </summary>
    /// <param name="trainerId">The trainer id</param>
    /// <param name="isOnline">The updated online status</param>
    /// <exception cref="ArgumentNullException" />
    public Task<Trainer> UpdateTrainerOnlineStatus(
        Guid trainerId,
        Guid gameId,
        bool isOnline);

    /// <summary>
    /// Searches for a trainer, then updates their password
    /// </summary>
    /// <param name="trainerId">The trainer id</param>
    /// <param name="password">The updated password</param>
    /// <exception cref="ArgumentNullException" />
    public Task<Trainer> UpdateTrainerPassword(
        Guid trainerId,
        Guid gameId,
        string password);

    /// <summary>
    /// Searches for a trainer using their id, then deletes it
    /// </summary>
    /// <param name="gameId">The game id</param>
    /// <param name="userId">The user's id</param>
    public Task DeleteTrainer(Guid gameId, Guid userId);

    /// <summary>
    /// Searches for all trainers using their game id, then deletes it
    /// </summary>
    /// <param name="gameId">The game id</param>
    public Task DeleteTrainersByGameId(Guid gameId);
}
