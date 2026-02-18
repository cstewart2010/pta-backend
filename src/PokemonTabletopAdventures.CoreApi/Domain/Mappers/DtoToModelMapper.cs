using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Npcs;
using PokemonTabletopAdventures.Models.Pokedex;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain.Mappers;

public class DtoToModelMapper : IDtoToModelMapper
{
    public async Task<PokemonForm> ParseFromDto(BasePokemonDto model)
    {
        return await Task.FromResult(new PokemonForm
        {
            BaseFormName = model.BaseFormName,
            DexNo = model.DexNo,
            Diet = model.Diet,
            EggGroups = model.EggGroups,
            EggHatchRate = model.EggHatchRate,
            EvolvesFrom = model.EvolvesFrom,
            Form = model.Form,
            GMaxMove = model.GMaxMove,
            Habitats = model.Habitats,
            LegendaryStats = model.LegendaryStats,
            Moves = model.Moves,
            Name = model.Name,
            NormalPortrait = model.NormalPortrait,
            Passives = model.Passives,
            PokemonStats = model.PokemonStats,
            Proficiencies = model.Proficiencies,
            Rarity = model.Rarity,
            ShinyPortrait = model.ShinyPortrait,
            Size = model.Size,
            Skills = model.Skills,
            SpecialFormName = model.SpecialFormName,
            Stage = model.Stage,
            Type = model.Type,
            Weight = model.Weight
        });
    }

    public async Task<Item> ParseFromDto(ItemDto itemModel)
    {
        return await Task.FromResult(new Item
        {
            Amount = itemModel.Amount,
            Effects = itemModel.Effects,
            Name = itemModel.Name,
            Type = itemModel.Type
        });
    }

    public async Task<Game> ParseFromDto(GameDto model, bool isGM, INpcService npcService, ISettingService settingService, ITrainerService trainerService)
    {
        var trainerModels = await trainerService.GetTrainersByGameId(model.GameId);
        var npcModels = isGM ? [] : await Task.WhenAll(model.NPCs.Select(async id => await npcService.GetNpc(id)));
        var settingModels = await settingService.GetAllSettings(model.GameId);
        return new Game
        {
            GameId = model.GameId,
            Nickname = model.Nickname,
            Trainers = trainerModels,
            Npcs = npcModels,
            Settings = settingModels,
            Logs = await Task.WhenAll(model.Logs.Select(ParseFromDto)),
            IsOnline = model.IsOnline,
        };
    }

    public async Task<Log> ParseFromDto(LogDto log)
    {
        return await Task.FromResult(new Log
        {
            Action = log.Action,
            LogTimestamp = log.LogTimestamp,
            User = log.User,
        });
    }

    public async Task<Npc> ParseFromDto(NpcDto npc, IPokemonService pokemonService)
    {
        var npcPokemon = await pokemonService.GetPokemonByTrainerId(npc.NPCId, npc.GameId, true);
        return new Npc
        {
            NpcId = npc.NPCId,
            GameId = npc.GameId,
            TrainerName = npc.TrainerName,
            Feats = npc.Feats,
            TrainerClasses = npc.TrainerClasses,
            TrainerStats = npc.TrainerStats,
            PokemonTeam = npcPokemon.Where(pokemon => pokemon.IsOnActiveTeam),
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

    public async Task<PokedexItem> ParseFromDto(PokeDexItemDto pokeDexItem)
    {
        return await Task.FromResult(new PokedexItem
        {
            DexNo = pokeDexItem.DexNo,
            GameId = pokeDexItem.GameId,
            IsCaught = pokeDexItem.IsCaught,
            IsSeen = pokeDexItem.IsSeen,
            TrainerId = pokeDexItem.TrainerId
        });
    }

    public async Task<Pokemon> ParseFromDto(PokemonDto pokemon)
    {
        return await Task.FromResult(new Pokemon
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
        });
    }

    public async Task<Setting> ParseFromDto(SettingDto model, bool isGM, Guid gameId, IShopService shopService)
    {
        var shopModels = await Task.WhenAll(model.Shops.Select(async id => await shopService.GetShopById(id, gameId)));
        return new Setting
        {
            SettingId = model.SettingId,
            Name = model.Name,
            IsActive = model.IsActive,
            Type = model.Type,
            Environment = model.Environment,
            Shops = shopModels.Where(shopModel => isGM || shopModel.IsActive),
            Participants = await Task.WhenAll(model.ActiveParticipants.Select(ParseFromDto)),
            GameId = gameId,
        };
    }

    public async Task<SettingParticipant> ParseFromDto(SettingParticipantModel dto)
    {
        return await Task.FromResult(new SettingParticipant
        {
            Health = dto.Health,
            Name = dto.Name,
            ParticipantId = dto.ParticipantId,
            Position = dto.Position,
            Speed = dto.Speed,
            Type = dto.Type
        });
    }

    public async Task<Shop> ParseFromDto(ShopDto dto)
    {
        var model = new Shop
        {
            ShopId = dto.ShopId,
            Name = dto.Name,
            Inventory = [],
            GameId = dto.GameId,
            IsActive = dto.IsActive
        };
        foreach (var pair in dto.Inventory)
        {
            model.Inventory.Add(pair.Key, await ParseFromDto(pair.Value));
        }

        return model;
    }

    public async Task<Ware> ParseFromDto(WareDto model)
    {
        return await Task.FromResult(new Ware
        {
            Cost = model.Cost,
            Effects = model.Effects,
            Quantity = model.Quantity,
            Type = model.Type
        });
    }

    public async Task<Trainer> ParseFromDto(TrainerDto trainer, IPokemonService pokemonService, IPokedexService pokedexService)
    {
        var trainerPokemon = await pokemonService.GetPokemonByTrainerId(trainer.TrainerId, trainer.GameId, false);
        var pokedex = (await pokedexService.GetTrainerPokeDex(trainer.TrainerId, trainer.GameId)).OrderBy(item => item.DexNo);
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
            PokemonTeam = trainerPokemon.Where(pokemon => pokemon.IsOnActiveTeam),
            PokemonHome = trainerPokemon.Where(pokemon => !pokemon.IsOnActiveTeam),
            PokeDex = pokedex,
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
            Items = await Task.WhenAll(trainer.Items.Select(ParseFromDto)),
            CurrentHP = trainer.CurrentHP,
            IsAllowed = trainer.IsAllowed,
            NewPokemon = [],
            Sprite = trainer.Sprite,
        };
    }

    public async Task<User> ParseFromDto(UserDto model)
    {
        return await Task.FromResult(new User
        {
            DateCreated = model.DateCreated,
            Games = model.Games,
            Messages = [..model.Messages],
            UserId = model.UserId,
            Username = model.Username,
            SiteRole = model.SiteRole,
            ActivityToken = model.ActivityToken
        });
    }

    public async Task<UserMessageThread> ParseFromDto(UserMessageThreadDto dto)
    {
        return new UserMessageThread
        {
            MessageId = dto.MessageId,
            Messages = await Task.WhenAll(dto.Messages.Select(ParseFromDto))
        };
    }

    public async Task<UserMessage> ParseFromDto(UserMessageDto dto)
    {
        return await Task.FromResult(new UserMessage
        {
            Message = dto.Message,
            Timestamp = dto.Timestamp,
            User = dto.User,
        });
    }
}
