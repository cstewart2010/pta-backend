using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Npcs;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IModelToDtoMapper
{
    public Task<BasePokemonDto> ParseFromModel(PokemonForm model);
    public Task<ItemDto> ParseFromModel(Item item);
    public Task<GameDto> ParseFromModel(Game game);
    public Task<LogDto> ParseFromModel(Log log);
    public Task<NpcDto> ParseFromModel(Npc npc);
    public Task<PokemonDto> ParseFromModel(Pokemon pokemon);
    public Task<SettingDto> ParseFromModel(Setting setting);
    public Task<SettingParticipantModel> ParseFromModel(SettingParticipant model);
    public Task<ShopDto> ParseFromModel(Shop shop);
    public Task<WareDto> ParseFromModel(Ware ware);
    public Task<TrainerDto> ParseFromModel(Trainer trainer);
    public Task<UserDto> ParseFromModel(User model);
    public Task<UserMessageThreadDto> ParseFromModel(UserMessageThread model);
    public Task<UserMessageDto> ParseFromModel(UserMessage model);
}
