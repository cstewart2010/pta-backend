using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Extensions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

public abstract class PtaControllerBase(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IGameService gameService,
    IDexService dexService,
    IPokedexService pokedexService,
    IEncryptionService encryptionService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper) : ControllerBase
{
    protected IUserService UserService { get; } = userService;
    protected ITrainerService TrainerService { get; } = trainerService;
    protected IPokemonService PokemonService { get; } = pokemonService;
    protected IGameService GameService { get; } = gameService;
    protected IDexService DexService { get; } = dexService;
    protected IPokedexService PokedexService { get; } = pokedexService;
    protected IDtoToModelMapper DtoToModelMapper { get; } = dtoToModelMapper;
    protected IModelToDtoMapper ModelToDtoMapper { get; } = modelToDtoMapper;

    protected async Task<Pokemon> BuildPokemon(
        Guid trainerId,
        Guid gameId,
        WildPokemon wild)
    {
        var pokemon = await BuildDefaultPokemon(wild);
        pokemon.TrainerId = trainerId;
        pokemon.OriginalTrainerId = trainerId;
        pokemon.GameId = gameId;
        return pokemon;
    }

    protected async Task<IEnumerable<Log>> AddItemsToTrainer(Trainer trainer, ICollection<Item> items)
    {
        var itemList = trainer.Items;
        foreach (var item in items)
        {
            trainer.Items = UpdateAllItemsWithAddition
            (
                [..itemList],
                item,
                trainer
            );
        }

        await TrainerService.UpdateTrainer(trainer);
        return items.Select(item => new Log
        {
            User = trainer.TrainerName,
            Action = $"added ({item.Amount}) {item.Name}",
            LogTimestamp = DateTimeOffset.Now
        });
    }

    protected async Task VerifyIdentity(
        string sessionAuth,
        Guid id)
    {
        var user = await UserService.GetUserById(id);
        Request.VerifyIdentity(user, encryptionService, sessionAuth);
    }

    protected async Task<bool> VerifyIdentity(
        string sessionAuth,
        Guid id,
        Guid gameId)
    {
        var user = await UserService.GetUserById(id);
        Request.VerifyIdentity(user, encryptionService, sessionAuth);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        return trainers.FirstOrDefault(trainer => trainer.TrainerId == id)?.IsGM == true;
    }

    protected async Task IsUserGM(
        Guid id,
        Guid gameId,
        string sessionAuth)
    {
        var trainer = await TrainerService.GetTrainerById(id, gameId);
        var user = await UserService.GetUserById(id);
        Request.IsUserGM(encryptionService, sessionAuth, user, trainer);
    }

    protected async Task AssignAuthAndToken(Guid id)
    {
        await Response.AssignAuthAndToken(encryptionService, UserService, id);
    }

    protected async Task<IEnumerable<Log>> RemoveItemsFromTrainer(Trainer trainer, ICollection<Item> items)
    {
        var itemList = trainer.Items;
        foreach (var item in items)
        {
            itemList = UpdateAllItemsWithReduction
            (
                [..itemList],
                item,
                trainer
            );
        }

        await TrainerService.UpdateTrainerItemList(trainer.TrainerId, trainer.GameId, itemList);
        return items.Select(item => new Log
        {
            User = trainer.TrainerName,
            Action = $"removed ({item.Amount}) {item.Name}",
            LogTimestamp = DateTimeOffset.Now,
        });
    }

    protected async Task IsGameAuthenticated(
        string gamePassword,
        Game game)
    {
        await encryptionService.VerifySecret(gamePassword, game.GameId);
    }

    protected async Task IsUserAuthenticated(LoginRequest request)
    {
        var user = await UserService.GetUserByUsername(request.Username);
        await encryptionService.VerifySecret(request.Password, request.Username);
        await UserService.UpdateUserOnlineStatus(user.UserId, true);
    }

    protected async Task<Trainer> BuildTrainer(
        Guid gameId,
        Guid userId,
        string username,
        bool isGM)
    {
        if (await TrainerService.GetTrainerByUsername(username, gameId) != null)
        {
            throw new DuplicateEntryException(typeof(Trainer));
        }

        var trainer = await CreateTrainer(gameId, userId, username);
        trainer.IsGM = isGM;
        trainer.IsAllowed = isGM;
        return trainer;
    }

    private async Task<Trainer> CreateTrainer(
        Guid gameId,
        Guid userId,
        string username)
    {
        var user = await UserService.GetUserById(userId);
        user.Games.Add(gameId);
        await UserService.UpdateUser(user);
        return new Trainer
        {
            GameId = gameId,
            TrainerId = userId,
            Honors = [],
            TrainerName = username,
            TrainerClasses = [],
            Feats = [],
            IsOnline = true,
            Items = [],
            TrainerStats = new Stats
            {
                HP = 20,
                Attack = 1,
                Defense = 1,
                SpecialAttack = 1,
                SpecialDefense = 1,
                Speed = 1
            },
            CurrentHP = 20,
            Origin = string.Empty,
            Age = 0,
            IsAllowed = false,
            Background = string.Empty,
            SeenTotal = 0,
            CaughtTotal = 0,
            Description = string.Empty,
            Gender = Gender.Genderless,
            Goals = string.Empty,
            Height = 0,
            IsComplete = false,
            IsGM = false,
            Level = 1,
            Money = 0,
            NewPokemon = [],
            Personality = string.Empty,
            PokeDex = [],
            PokemonHome = [],
            PokemonTeam = [],
            Species = string.Empty,
            Sprite = Sprites.AceTrainer,
            TrainerSkills = [],
            Weight = 0
        };
    }

    private async Task<Pokemon> BuildDefaultPokemon(WildPokemon wild)
    {
        var random = new Random();
        if (!Enum.IsDefined(wild.Gender))
        {
            var genders = Enum.GetValues<Gender>();
            wild.Gender = genders[random.Next(genders.Length)];
        }

        if (!Enum.IsDefined(wild.Nature))
        {
            var natures = Enum.GetValues<Nature>();
            wild.Nature = natures[random.Next(1, natures.Length)];
        }

        if (!Enum.IsDefined(wild.Status))
        {
            wild.Status = Status.Normal;
        }

        var form = wild.Form.Replace('_', '/');
        var pokemon = await DexService.GetNewPokemon(
            wild.Pokemon,
            wild.Nature,
            wild.Gender,
            wild.Status,
            null,
            form);

        if (wild.ForceShiny)
        {
            pokemon.IsShiny = true;
        }
        pokemon.Pokeball = nameof(Pokeball.Basic_Ball).Replace("_", "");
        return pokemon;
    }

    #region Helper methods
    internal static RetrieveLogsResponse CreateRetrieveLogsResponse(Game game, int count)
    {
        if (count > game.Logs.Count)
        {
            count = game.Logs.Count;
        }

        var response = new RetrieveLogsResponse { LogPages = [] };
        var logs = game.Logs.OrderByDescending(log => log.LogTimestamp).ToList();
        for (var i = 0; i < count; i += 50)
        {
            response.LogPages.Add(GetPage(logs, i, 50));
        }

        return response;
    }

    private static IEnumerable<Log> GetPage(
        ICollection<Log> source,
        int offset,
        int limit)
    {
        if (offset < 0 || offset >= source.Count() || limit < 0)
        {
            return [];
        }

        if (limit >= source.Count())
        {
            return source.Skip(offset);
        }

        if (offset + limit > source.Count())
        {
            limit = source.Count() - offset;
        }

        return source.Skip(offset).Take(limit);
    }

    private static List<Item> UpdateAllItemsWithAddition(
        List<Item> itemList,
        Item itemToken,
        Trainer trainer)
    {
        var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemToken.Name, StringComparison.CurrentCultureIgnoreCase));
        if (item == null)
        {
            itemList.Add(itemToken);
        }
        else
        {
            itemList = [.. trainer.Items.Select(x => UpdateItemWithAddition(x, itemToken))];
        }

        return itemList;
    }

    private static Item UpdateItemWithAddition(
        Item item,
        Item newItem)
    {
        if (item.Name == newItem.Name)
        {
            item.Amount = item.Amount + newItem.Amount > 100
                ? 100
                : item.Amount + newItem.Amount;
        }

        return item;
    }

    private static List<Item> UpdateAllItemsWithReduction(
        List<Item> itemList,
        Item itemToken,
        Trainer trainer)
    {
        itemList = [.. trainer.Items
            .Select(x => UpdateItemWithReduction(x, itemToken))
            .Where(x => x.Amount > 0)];

        return itemList;
    }

    private static Item UpdateItemWithReduction(
        Item item,
        Item newItem)
    {
        if (item.Name == newItem.Name)
        {
            if (item.Amount > 0)
            {
                return item;
            }
            item.Amount -= newItem.Amount;
        }

        throw new ItemNotFoundException(item.Name);
    }
#endregion
}
