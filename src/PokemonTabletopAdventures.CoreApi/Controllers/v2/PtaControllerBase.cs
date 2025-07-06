using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.DTOs.Enums;
using PokemonTabletopAdventures.CoreApi.DTOs.Games;
using PokemonTabletopAdventures.CoreApi.DTOs.Npcs;
using PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;
using PokemonTabletopAdventures.CoreApi.DTOs.Settings;
using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.DTOs.Users;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Extensions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

public abstract class PtaControllerBase(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IGameService gameService,
    IDexService dexService,
    IPokedexService pokedexService,
    IEncryptionService encryptionService) : ControllerBase
{
    private readonly IEncryptionService _encryptionService = encryptionService;
    public IUserService UserService { get; } = userService;
    public ITrainerService TrainerService { get; } = trainerService;
    public IPokemonService PokemonService { get; } = pokemonService;
    public IGameService GameService { get; } = gameService;
    public IDexService DexService { get; } = dexService;
    public IPokedexService PokedexService { get; } = pokedexService;

    protected async Task<PokemonModel> BuildPokemon(
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

    protected async Task<ICollection<Game>> ParseFromModel(
        IEnumerable<GameModel> models,
        bool isGM,
        INpcService npcService,
        ISettingService settingService,
        IShopService shopService)
    {
        var games = await Task.WhenAll(models.Select(async model =>
        {
            var trainerModels = await TrainerService.GetTrainersByGameId(model.GameId);
            var npcModels = isGM ? [] : await Task.WhenAll(model.NPCs.Select(async id => await npcService.GetNpc(id)));
            var settingModels = await settingService.GetAllSettings(model.GameId);
            var trainers = await Task.WhenAll(trainerModels.Select(async trainer => await ParseFromModel(trainer)));
            var npcs = await Task.WhenAll(npcModels.Select(async npc => await ParseFromModel(npc)));
            var settings = await Task.WhenAll(settingModels.Select(async setting => await Setting.ParseFromModel(setting, isGM, model.GameId, shopService)));
            return new Game
            {
                GameId = model.GameId,
                Nickname = model.Nickname,
                Trainers = trainers,
                Npcs = npcs,
                Settings = settings,
                Logs = model.Logs.Select(ParseFromModel)
            };
        }));

        return games;
    }

    protected async Task<Npc> ParseFromModel(NpcModel npc)
    {
        var npcPokemon = await PokemonService.GetPokemonByTrainerId(npc.NPCId);
        return new Npc
        {
            NpcId = npc.NPCId,
            GameId = npc.GameId,
            TrainerName = npc.TrainerName,
            Feats = npc.Feats,
            TrainerClasses = npc.TrainerClasses,
            TrainerStats = npc.TrainerStats,
            PokemonTeam = npcPokemon.Where(pokemon => pokemon.IsOnActiveTeam).Select(ParseFromModel),
            Level = npc.Level,
            TrainerSkills = npc.TrainerSkills,
            Gender = npc.Gender,
            Height = npc.Height,
            Weight = npc.Weight,
            Description = npc.Description,
            Personality = npc.Personality,
            Background = npc.Background,
            Goals = npc.Goals,
            Species = npc.Species,
            Sprite = npc.Sprite,
            Age = npc.Age,
        };
    }

    protected async Task<NpcModel> ParseBackToModel(Npc npc, Guid gameId, INpcService npcService)
    {
        var model = await npcService.GetNpc(npc.NpcId);
        model.TrainerName = npc.TrainerName;
        model.Feats = npc.Feats;
        model.TrainerClasses = npc.TrainerClasses;
        model.TrainerStats = npc.TrainerStats;
        model.Level = npc.Level;
        model.TrainerSkills = npc.TrainerSkills;
        model.Gender = npc.Gender;
        model.Height = npc.Height;
        model.Weight = npc.Weight;
        model.Description = npc.Description;
        model.Personality = npc.Personality;
        model.Background = npc.Background;
        model.Goals = npc.Goals;
        model.Species = npc.Species;
        model.Sprite = npc.Sprite;
        model.Age = npc.Age;
        model.GameId = gameId;
        return model;
    }

    protected Pokemon ParseFromModel(PokemonModel pokemon)
    {
        return new Pokemon
        {
            AlternateForms = pokemon.AlternateForms,
            IsOnActiveTeam = pokemon.IsOnActiveTeam,
            CanEvolve = pokemon.CanEvolve,
            CurrentHP = pokemon.CurrentHP,
            DexNo = pokemon.DexNo,
            Diet = pokemon.Diet,
            EggGroups = pokemon.EggGroups,
            EggHatchRate = pokemon.EggHatchRate,
            EvolvedFrom = pokemon.EvolvedFrom,
            Form = pokemon.Form,
            GameId = pokemon.GameId,
            Gender = pokemon.Gender,
            GMaxMove = pokemon.GMaxMove,
            Habitats = pokemon.Habitats,
            IsShiny = pokemon.IsShiny,
            LegendaryStats = pokemon.LegendaryStats,
            Moves = pokemon.Moves,
            Nature = pokemon.Nature,
            Nickname = pokemon.Nickname,
            NormalPortrait = pokemon.NormalPortrait,
            OriginalTrainerId = pokemon.OriginalTrainerId,
            Passives = pokemon.Passives,
            Pokeball = pokemon.Pokeball,
            PokemonId = pokemon.PokemonId,
            PokemonStats = pokemon.PokemonStats,
            PokemonStatus = pokemon.PokemonStatus,
            Proficiencies = pokemon.Proficiencies,
            Rarity = pokemon.Rarity,
            ShinyPortrait = pokemon.ShinyPortrait,
            Size = pokemon.Size,
            Skills = pokemon.Skills,
            SpeciesName = pokemon.SpeciesName,
            TrainerId = pokemon.TrainerId,
            Type = pokemon.Type,
            Weight = pokemon.Weight
        };
    }

    protected PokedexItem ParseFromModel(PokeDexItemModel pokeDexItem)
    {
        return new PokedexItem
        {
            DexNo = pokeDexItem.DexNo,
            GameId = pokeDexItem.GameId,
            IsCaught = pokeDexItem.IsCaught,
            IsSeen = pokeDexItem.IsSeen,
            TrainerId = pokeDexItem.TrainerId
        };
    }

    protected PokemonModel ParseBackToModel(Pokemon pokemon)
    {
        var model = PokemonService.GetPokemonById(pokemon.PokemonId);
        return new PokemonModel
        {
            AlternateForms = pokemon.AlternateForms,
            IsOnActiveTeam = pokemon.IsOnActiveTeam,
            CanEvolve = pokemon.CanEvolve,
            CurrentHP = pokemon.CurrentHP,
            DexNo = pokemon.DexNo,
            Diet = pokemon.Diet,
            EggGroups = pokemon.EggGroups,
            EggHatchRate = pokemon.EggHatchRate,
            EvolvedFrom = pokemon.EvolvedFrom,
            Form = pokemon.Form,
            GameId = pokemon.GameId,
            Gender = pokemon.Gender,
            GMaxMove = pokemon.GMaxMove,
            Habitats = pokemon.Habitats,
            IsShiny = pokemon.IsShiny,
            LegendaryStats = pokemon.LegendaryStats,
            Moves = pokemon.Moves,
            Nature = pokemon.Nature,
            Nickname = pokemon.Nickname,
            NormalPortrait = pokemon.NormalPortrait,
            OriginalTrainerId = pokemon.OriginalTrainerId,
            Passives = pokemon.Passives,
            Pokeball = pokemon.Pokeball,
            PokemonId = pokemon.PokemonId,
            PokemonStats = pokemon.PokemonStats,
            PokemonStatus = pokemon.PokemonStatus,
            Proficiencies = pokemon.Proficiencies,
            Rarity = pokemon.Rarity,
            ShinyPortrait = pokemon.ShinyPortrait,
            Size = pokemon.Size,
            Skills = pokemon.Skills,
            SpeciesName = pokemon.SpeciesName,
            TrainerId = pokemon.TrainerId,
            Type = pokemon.Type,
            Weight = pokemon.Weight
        };
    }


    internal async Task<Trainer> ParseFromModel(TrainerModel trainer)
    {
        var trainerPokemon = await PokemonService.GetPokemonByTrainerId(trainer.TrainerId, trainer.GameId);
        var pokedex = (await PokedexService.GetTrainerPokeDex(trainer.TrainerId, trainer.GameId)).OrderBy(item => item.DexNo);
        var caught = pokedex.Count(dexItem => dexItem.IsCaught);
        return new Trainer
        {
            TrainerId = trainer.TrainerId,
            TrainerName = trainer.TrainerName,
            IsGM = trainer.IsGM,
            IsOnline = trainer.IsOnline,
            Feats = trainer.Feats,
            GameId = trainer.GameId,
            Honors = trainer.Honors,
            Money = trainer.Money,
            Origin = trainer.Origin,
            TrainerClasses = trainer.TrainerClasses,
            TrainerStats = trainer.TrainerStats,
            IsComplete = trainer.IsComplete,
            PokemonTeam = trainerPokemon.Where(pokemon => pokemon.IsOnActiveTeam).Select(ParseFromModel),
            PokemonHome = trainerPokemon.Where(pokemon => !pokemon.IsOnActiveTeam).Select(ParseFromModel),
            PokeDex = pokedex.Select(ParseFromModel).OrderBy(item => item.DexNo),
            SeenTotal = pokedex.Count(dexItem => dexItem.IsSeen),
            CaughtTotal = caught,
            Level = trainer.Honors.Count() + caught / 30 + 1,
            TrainerSkills = trainer.TrainerSkills,
            Age = trainer.Age,
            Gender = trainer.Gender,
            Height = trainer.Height,
            Weight = trainer.Weight,
            Description = trainer.Description,
            Personality = trainer.Personality,
            Background = trainer.Background,
            Goals = trainer.Goals,
            Species = trainer.Species,
            Items = trainer.Items,
            CurrentHP = trainer.CurrentHP,
            IsAllowed = trainer.IsAllowed,
            NewPokemon = [],
            Sprite = trainer.Sprite,
        };
    }

    internal async Task<TrainerModel> ParseBackToModel(Trainer trainer)
    {
        var model = await TrainerService.GetTrainerById(trainer.TrainerId, trainer.GameId);
        trainer.TrainerName = trainer.TrainerName;
        trainer.Feats = trainer.Feats;
        trainer.Money = trainer.Money;
        trainer.Origin = trainer.Origin;
        trainer.TrainerClasses = trainer.TrainerClasses;
        trainer.TrainerStats = trainer.TrainerStats;
        trainer.TrainerSkills = trainer.TrainerSkills;
        trainer.Age = trainer.Age;
        trainer.Gender = trainer.Gender;
        trainer.Height = trainer.Height;
        trainer.Weight = trainer.Weight;
        trainer.Description = trainer.Description;
        trainer.Personality = trainer.Personality;
        trainer.Background = trainer.Background;
        trainer.Goals = trainer.Goals;
        trainer.Items = [.. trainer.Items];
        trainer.Species = trainer.Species;
        trainer.CurrentHP = trainer.CurrentHP;
        trainer.Sprite = trainer.Sprite;
        if (!(trainer.IsComplete || string.IsNullOrEmpty(trainer.Origin)))
        {
            trainer.IsComplete = true;
        }
        return model;
    }

    protected async Task<IEnumerable<LogModel>> AddItemsToTrainer(TrainerModel trainer, IEnumerable<ItemModel> items)
    {
        var itemList = trainer.Items;
        foreach (var item in items)
        {
            trainer.Items = UpdateAllItemsWithAddition
            (
                itemList,
                item,
                trainer
            );
        }

        await TrainerService.UpdateTrainer(trainer);
        return items.Select(item => new LogModel
        (
            user: trainer.TrainerName,
            action: $"added ({item.Amount}) {item.Name}"
        ));
    }

    protected async Task VerifyIdentity(
        string accessToken,
        string sessionAuth,
        Guid id)
    {
        var user = await UserService.GetUserById(id);
        Request.VerifyIdentity(user, _encryptionService, accessToken, sessionAuth);
    }

    protected async Task IsUserGM(
        Guid id,
        Guid gameId,
        string accessToken,
        string sessionAuth)
    {
        var trainer = await TrainerService.GetTrainerById(id, gameId);
        var user = await UserService.GetUserById(id);
        Request.IsUserGM(_encryptionService, accessToken, sessionAuth, user, trainer);
    }

    protected async Task AssignAuthAndToken(Guid id)
    {
        await Response.AssignAuthAndToken(_encryptionService, UserService, id);
    }

    protected async Task RefreshToken(Guid id)
    {
        await Response.RefreshToken(_encryptionService, UserService, id);
    }

    protected async Task<IEnumerable<LogModel>> RemoveItemsFromTrainer(TrainerModel trainer, IEnumerable<ItemModel> items)
    {
        var itemList = trainer.Items;
        foreach (var item in items)
        {
            itemList = UpdateAllItemsWithReduction
            (
                itemList,
                item,
                trainer
            );
        }

        await TrainerService.UpdateTrainerItemList(trainer.TrainerId,trainer.GameId,itemList);
        return items.Select(item => new LogModel
        (
            user: trainer.TrainerName,
            action: $"removed ({item.Amount}) {item.Name}"
        ));
    }

    protected async Task IsGameAuthenticated(
        string gamePassword,
        GameModel game)
    {
        await _encryptionService.VerifySecret(gamePassword, game.PasswordHash);
    }

    protected async Task IsUserAuthenticated(PutLoginRequest request)
    {
        var user = await UserService.GetUserByUsername(request.Username) ?? throw new PtaUnauthorizedException("No username found with provided");
        await _encryptionService.VerifySecret(request.Password, user.PasswordHash);
        await UserService.UpdateUserOnlineStatus(user.UserId, true);
    }

    protected async Task<TrainerModel> BuildTrainer(
        Guid gameId,
        Guid userId,
        string username,
        bool isGM)
    {
        if (await TrainerService.GetTrainerByUsername(username, gameId) != null)
        {
            throw new DuplicateEntryException(typeof(TrainerModel));
        }

        var trainer = await CreateTrainer(gameId, userId, username);
        trainer.IsGM = isGM;
        trainer.IsAllowed = isGM;
        return trainer;
    }

    private async Task<TrainerModel> CreateTrainer(
        Guid gameId,
        Guid userId,
        string username)
    {
        var user = await UserService.GetUserById(userId);
        user.Games.Add(gameId);
        await UserService.UpdateUser(user);
        return new TrainerModel
        {
            GameId = gameId,
            TrainerId = userId,
            Honors = [],
            TrainerName = username,
            TrainerClasses = [],
            Feats = [],
            IsOnline = true,
            Items = [],
            TrainerStats = new StatsModel
            {
                HP = 20,
                Attack = 1,
                Defense = 1,
                SpecialAttack = 1,
                SpecialDefense = 1,
                Speed = 1
            },
            CurrentHP = 20,
            Origin = string.Empty
        };
    }

    private async Task<PokemonModel> BuildDefaultPokemon(WildPokemon wild)
    {
        var random = new Random();
        if (!Enum.TryParse(wild.Gender, true, out Gender gender))
        {
            var genders = Enum.GetValues<Gender>();
            gender = genders[random.Next(genders.Length)];
        }

        if (!Enum.TryParse(wild.Nature, true, out Nature nature))
        {
            var natures = Enum.GetValues<Nature>();
            nature = natures[random.Next(1, natures.Length)];
        }

        if (!Enum.TryParse(wild.Status, true, out Status status))
        {
            status = Status.Normal;
        }

        var form = wild.Form.Replace('_', '/');
        var pokemon = await DexService.GetNewPokemon(
            wild.Pokemon,
            nature,
            gender,
            status,
            null,
            form);

        if (wild.ForceShiny)
        {
            pokemon.IsShiny = true;
        }
        pokemon.Pokeball = Pokeball.Basic_Ball.ToString();
        return pokemon;
    }

    #region Helper functions
    internal static RetrieveLogsResponse CreateRetrieveLogsResponse(GameModel game, int count)
    {
        if (count > game.Logs.Count)
        {
            count = game.Logs.Count;
        }

        var response = new RetrieveLogsResponse { LogPages = [] };
        if (game.Logs != null)
        {
            var logs = game.Logs.OrderByDescending(log => log.LogTimestamp);
            for (int i = 0; i < count; i += 50)
            {
                response.LogPages.Add(GetPage(logs, i, 50));
            }
        }

        return response;
    }

    internal static LogModel ParseBackToModel(Log log)
    {
        return new LogModel(log.User, log.Action);
    }

    private static IEnumerable<Log> GetPage(
        IEnumerable<LogModel> source,
        int offset,
        int limit)
    {
        if (offset < 0 || offset >= source.Count() || limit < 0)
        {
            return [];
        }

        if (limit >= source.Count())
        {
            return source.Skip(offset).Select(ParseFromModel);
        }

        if (offset + limit > source.Count())
        {
            limit = source.Count() - offset;
        }

        return source.Skip(offset).Take(limit).Select(ParseFromModel);
    }

    private static Log ParseFromModel(LogModel log)
    {
        return new Log
        {
            LogTimestamp = log.LogTimestamp,
            Action = log.Action,
            User = log.User,
        };
    }

    private static List<ItemModel> UpdateAllItemsWithAddition(
        List<ItemModel> itemList,
        ItemModel itemToken,
        TrainerModel trainer)
    {
        var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemToken.Name, StringComparison.CurrentCultureIgnoreCase));
        if (item == null)
        {
            itemList.Add(itemToken);
        }
        else
        {
            itemList = [.. trainer.Items.Select(item => UpdateItemWithAddition(item, itemToken))];
        }

        return itemList;
    }

    private static ItemModel UpdateItemWithAddition(
        ItemModel item,
        ItemModel newItem)
    {
        if (item.Name == newItem.Name)
        {
            item.Amount = item.Amount + newItem.Amount > 100
                ? 100
                : item.Amount + newItem.Amount;
        }

        return item;
    }

    private static List<ItemModel> UpdateAllItemsWithReduction(
        List<ItemModel> itemList,
        ItemModel itemToken,
        TrainerModel trainer)
    {
        var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemToken.Name, StringComparison.CurrentCultureIgnoreCase));
        if ((item?.Amount ?? 0) >= itemToken.Amount)
        {
            itemList = [.. trainer.Items
                .Select(item => UpdateItemWithReduction(item, itemToken))
                .Where(item => item.Amount > 0)];
        }

        return itemList;
    }

    private static ItemModel UpdateItemWithReduction(
        ItemModel item,
        ItemModel newItem)
    {
        if (item.Name == newItem.Name)
        {
            item.Amount -= newItem.Amount;
        }

        return item;
    }
#endregion
}
