using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Domain;
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
            var trainers = await Task.WhenAll(trainerModels.Select(async trainer => await Trainer.ParseFromModel(trainer, PokemonService, PokedexService)));
            var npcs = await Task.WhenAll(npcModels.Select(async npc => await ParseFromModel(npc)));
            var settings = await Task.WhenAll(settingModels.Select(async setting => await Setting.ParseFromModel(setting, isGM, model.GameId, shopService)));
            return new Game
            {
                GameId = model.GameId,
                Nickname = model.Nickname,
                Trainers = trainers,
                Npcs = npcs,
                Settings = settings,
                Logs = model.Logs
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
}
