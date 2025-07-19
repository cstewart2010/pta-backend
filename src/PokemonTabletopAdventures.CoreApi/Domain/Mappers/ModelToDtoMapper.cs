using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Npcs;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Domain.Mappers;

public class ModelToDtoMapper : IModelToDtoMapper
{
    public async Task<BasePokemonDto> ParseFromModel(PokemonForm model)
    {
        return await Task.FromResult(new BasePokemonDto
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

    public async Task<ItemDto> ParseFromModel(Item item)
    {
        return await Task.FromResult(new ItemDto
        {
            Amount = item.Amount,
            Effects = item.Effects,
            Name = item.Name,
            Type = item.Type
        });
    }

    public async Task<GameDto> ParseFromModel(Game game)
    {
        return await Task.FromResult(new GameDto
        {
            GameId = game.GameId,
            IsOnline = false,
            Logs = await Task.WhenAll(game.Logs.Select(ParseFromModel)),
            Nickname = game.Nickname,
            NPCs = [.. game.Npcs.Select(npc => npc.NpcId)],
        });
    }

    public async Task<LogDto> ParseFromModel(Log log)
    {
        return await Task.FromResult(new LogDto(log.User, log.Action));
    }

    public async Task<NpcDto> ParseFromModel(Npc npc)
    {
        return await Task.FromResult(new NpcDto
        {
            Age = npc.Age,
            Background = npc.Background,
            CurrentHP = npc.CurrentHP,
            Description = npc.Description,
            Feats = npc.Feats,
            GameId = npc.GameId,
            Gender = npc.Gender,
            Goals = npc.Goals,
            Height = npc.Height,
            Level = npc.Level,
            NPCId = npc.NpcId,
            Personality = npc.Personality,
            TrainerClasses = npc.TrainerClasses,
            Species = npc.Species,
            Sprite = npc.Sprite,
            TrainerName = npc.TrainerName,
            TrainerSkills = npc.TrainerSkills,
            TrainerStats = npc.TrainerStats,
            Weight = npc.Weight
        });
    }

    public async Task<PokemonDto> ParseFromModel(Pokemon pokemon)
    {
        return await Task.FromResult(new PokemonDto
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
            Weight = pokemon.Weight,
            CatchRate = pokemon.CatchRate
        });
    }

    public async Task<SettingDto> ParseFromModel(Setting setting)
    {
        return await Task.FromResult(new SettingDto
        {
            ActiveParticipants = await Task.WhenAll(setting.Participants.Select(ParseFromModel)),
            IsActive = setting.IsActive,
            Environment = setting.Environment,
            GameId = setting.GameId,
            Name = setting.Name,
            SettingId = setting.SettingId,
            Shops = setting.Shops.Select(shop => shop.ShopId),
            Type = setting.Type,
        });
    }

    public async Task<SettingParticipantModel> ParseFromModel(SettingParticipant model)
    {
        return await Task.FromResult(new SettingParticipantModel
        {
            Health = model.Health,
            Name = model.Name,
            ParticipantId = model.ParticipantId,
            Position = model.Position,
            Speed = model.Speed,
            Type = model.Type
        });
    }

    public async Task<ShopDto> ParseFromModel(Shop shop)
    {
        var dto = new ShopDto
        {
            ShopId = shop.ShopId,
            Name = shop.Name,
            Inventory = []
        };

        foreach (var pair in shop.Inventory)
        {
            dto.Inventory.Add(pair.Key, await ParseFromModel(pair.Value));
        }
        return await Task.FromResult(dto);
    }

    public async Task<WareDto> ParseFromModel(Ware ware)
    {
        return await Task.FromResult(new WareDto
        {
            Cost = ware.Cost,
            Effects = ware.Effects,
            Quantity = ware.Quantity,
            Type = ware.Type
        });
    }

    public async Task<TrainerDto> ParseFromModel(Trainer trainer)
    {
        var dto = new TrainerDto
        {
            TrainerName = trainer.TrainerName,
            Feats = trainer.Feats,
            Money = trainer.Money,
            Origin = trainer.Origin,
            TrainerClasses = trainer.TrainerClasses,
            TrainerStats = trainer.TrainerStats,
            TrainerSkills = trainer.TrainerSkills,
            Age = trainer.Age,
            IsAllowed = trainer.IsAllowed,
            Background = trainer.Background,
            CurrentHP = trainer.CurrentHP,
            Description = trainer.Description,
            GameId = trainer.GameId,
            Gender = trainer.Gender,
            Goals = trainer.Goals,
            Height = trainer.Height,
            Honors = trainer.Honors,
            IsGM = trainer.IsGM,
            IsOnline = trainer.IsOnline,
            Items = [.. await Task.WhenAll(trainer.Items.Select(ParseFromModel))],
            Personality = trainer.Personality,
            Species = trainer.Species,
            Sprite = trainer.Sprite,
            TrainerId = trainer.TrainerId,
            Weight = trainer.Weight
        };
        if (!(trainer.IsComplete || string.IsNullOrEmpty(trainer.Origin)))
        {
            dto.IsComplete = true;
        }
        return await Task.FromResult(dto);
    }

    public async Task<UserDto> ParseFromModel(User model)
    {
        return await Task.FromResult(new UserDto
        {
            DateCreated = model.DateCreated,
            ActivityToken = "",
            Games = [.. model.Games],
            IsOnline = false,
            Messages = model.Messages,
            PasswordHash = "",
            SiteRole = model.SiteRole,
            UserId = model.UserId,
            Username = model.Username
        });
    }

    public async Task<UserMessageThreadDto> ParseFromModel(UserMessageThread model)
    {
        return await Task.FromResult(new UserMessageThreadDto
        {
            MessageId = model.MessageId,
            Messages = await Task.WhenAll(model.Messages.Select(ParseFromModel))
        });
    }

    public async Task<UserMessageDto> ParseFromModel(UserMessage model)
    {
        return await Task.FromResult(new UserMessageDto
        {
            Message = model.Message,
            Timestamp = model.Timestamp,
            User = model.User,
        });
    }
}
