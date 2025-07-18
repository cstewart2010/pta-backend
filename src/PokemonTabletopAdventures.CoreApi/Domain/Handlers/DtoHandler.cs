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

namespace PokemonTabletopAdventures.CoreApi.Domain.Handlers
{
    internal static class DtoHandler
    {
        public static PokemonForm ParseFromDto(BasePokemonDto model)
        {
            return new PokemonForm
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
            };
        }

        public static BasePokemonDto ParseFromModel(PokemonForm model)
        {
            return new BasePokemonDto
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
            };
        }

        public static async Task<Game> ParseFromDto(
            GameDto model,
            bool isGM,
            INpcService npcService,
            ISettingService settingService,
            ITrainerService trainerService)
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
                Logs = [..model.Logs.Select(ParseFromDto)],
                IsOnline = model.IsOnline,
            };
        }

        public static GameDto ParseFromModel(Game game)
        {
            return new GameDto
            {
                GameId = game.GameId,
                IsOnline = false,
                Logs = [.. game.Logs.Select(ParseFromModel)],
                Nickname = game.Nickname,
                NPCs = [.. game.Npcs.Select(npc => npc.NpcId)],
            };
        }

        public static ItemDto ParseFromModel(Item item)
        {
            return new ItemDto
            {
                Amount = item.Amount,
                Effects = item.Effects,
                Name = item.Name,
                Type = item.Type
            };
        }

        public static Item ParseFromDto(ItemDto itemModel)
        {
            return new Item
            {
                Amount = itemModel.Amount,
                Effects = itemModel.Effects,
                Name = itemModel.Name,
                Type = itemModel.Type
            };
        }

        public static Log ParseFromDto(LogDto log)
        {
            return new Log
            {
                Action = log.Action,
                LogTimestamp = log.LogTimestamp,
                User = log.User,
            };
        }

        public static LogDto ParseFromModel(Log log)
        {
            return new LogDto(log.User, log.Action);
        }

        public static async Task<Npc> ParseFromDto(
            NpcDto npc,
            IPokemonService pokemonService)
        {
            var npcPokemon = await pokemonService.GetPokemonByTrainerId(npc.NPCId);
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

        public static NpcDto ParseFromModel(Npc npc)
        {
            return new NpcDto
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
            };
        }

        public static Pokemon ParseFromDto(PokemonDto pokemon)
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

        public static PokedexItem ParseFromDto(PokeDexItemDto pokeDexItem)
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

        public static PokemonDto ParseFromModel(Pokemon pokemon)
        {
            return new PokemonDto
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
            };
        }

        public static SettingDto ParseFromModel(Setting setting)
        {
            return new SettingDto
            {
                ActiveParticipants = setting.Participants.Select(ParseFromModel),
                IsActive = setting.IsActive,
                Environment = setting.Environment,
                GameId = setting.GameId,
                Name = setting.Name,
                SettingId = setting.SettingId,
                Shops = setting.Shops.Select(shop => shop.ShopId),
                Type = setting.Type,
            };
        }

        public static async Task<Setting> ParseFromDto(
            SettingDto model,
            bool isGM,
            Guid gameId,
            IShopService shopService)
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
                Participants = [..model.ActiveParticipants.Select(ParseFromDto)],
                GameId = gameId,
            };
        }

        public static Shop ParseFromDto(ShopDto model)
        {
            return new Shop
            {
                ShopId = model.ShopId,
                Name = model.Name,
                Inventory = model.Inventory.ToDictionary(x => x.Key, x => ParseFromDto(x.Value)),
            };
        }

        public static ShopDto ParseFromModel(Shop shop)
        {
            return new ShopDto
            {
                ShopId = shop.ShopId,
                Name = shop.Name,
                Inventory = shop.Inventory.ToDictionary(x => x.Key, x => ParseFromModel(x.Value)),
            };
        }

        public static Ware ParseFromDto(WareDto model)
        {
            return new Ware
            {
                Cost = model.Cost,
                Effects = model.Effects,
                Quantity = model.Quantity,
                Type = model.Type
            };
        }

        public static WareDto ParseFromModel(Ware ware)
        {
            return new WareDto
            {
                Cost = ware.Cost,
                Effects = ware.Effects,
                Quantity = ware.Quantity,
                Type = ware.Type
            };
        }

        public static async Task<Trainer> ParseFromDto(
            TrainerDto trainer,
            IPokemonService pokemonService,
            IPokedexService pokedexService)
        {
            var trainerPokemon = await pokemonService.GetPokemonByTrainerId(trainer.TrainerId, trainer.GameId);
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
                Items = trainer.Items.Select(ParseFromDto),
                CurrentHP = trainer.CurrentHP,
                IsAllowed = trainer.IsAllowed,
                NewPokemon = [],
                Sprite = trainer.Sprite,
            };
        }

        public static TrainerDto ParseFromModel(Trainer trainer)
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
                Items = [.. trainer.Items.Select(ParseFromModel)],
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
            return dto;
        }

        public static User ParseFromDto(UserDto model)
        {
            return new User
            {
                DateCreated = model.DateCreated,
                Games = model.Games,
                Messages = [..model.Messages],
                UserId = model.UserId,
                Username = model.Username,
                SiteRole = model.SiteRole,
                ActivityToken = model.ActivityToken
            };
        }

        public static UserDto ParseFromModel(User model)
        {
            return new UserDto
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
            };
        }

        public static UserMessageThread ParseFromDto(UserMessageThreadDto dto)
        {
            return new UserMessageThread
            {
                MessageId = dto.MessageId,
                Messages = [.. dto.Messages.Select(ParseFromDto)]
            };
        }

        public static UserMessageThreadDto ParseFromModel(UserMessageThread model)
        {
            return new UserMessageThreadDto
            {
                MessageId = model.MessageId,
                Messages = model.Messages.Select(ParseFromModel)
            };
        }

        public static UserMessage ParseFromDto(UserMessageDto dto)
        {
            return new UserMessage
            {
                Message = dto.Message,
                Timestamp = dto.Timestamp,
                User = dto.User,
            };
        }

        public static UserMessageDto ParseFromModel(UserMessage model)
        {
            return new UserMessageDto
            {
                Message = model.Message,
                Timestamp = model.Timestamp,
                User = model.User,
            };
        }

        public static SettingParticipantModel ParseFromModel(SettingParticipant model)
        {
            return new SettingParticipantModel
            {
                Health = model.Health,
                Name = model.Name,
                ParticipantId = model.ParticipantId,
                Position = model.Position,
                Speed = model.Speed,
                Type = model.Type
            };
        }

        public static SettingParticipant ParseFromDto(SettingParticipantModel dto)
        {
            return new SettingParticipant
            {
                Health = dto.Health,
                Name = dto.Name,
                ParticipantId = dto.ParticipantId,
                Position = dto.Position,
                Speed = dto.Speed,
                Type = dto.Type
            };
        }
    }
}
