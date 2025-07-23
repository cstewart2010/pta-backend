using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Npcs;
using PokemonTabletopAdventures.Models.Pokedex;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IDtoToModelMapper
{
    public Task<PokemonForm> ParseFromDto(BasePokemonDto model);

    public Task<Item> ParseFromDto(ItemDto itemModel);

    public Task<Game> ParseFromDto(GameDto model, bool isGM, INpcService npcService, ISettingService settingService, ITrainerService trainerService);

    public Task<Log> ParseFromDto(LogDto log);

    public Task<Npc> ParseFromDto(NpcDto npc, IPokemonService pokemonService);

    public Task<PokedexItem> ParseFromDto(PokeDexItemDto pokeDexItem);

    public Task<Pokemon> ParseFromDto(PokemonDto pokemon);

    public Task<Setting> ParseFromDto(SettingDto model, bool isGM, Guid gameId, IShopService shopService);

    public Task<SettingParticipant> ParseFromDto(SettingParticipantModel dto);

    public Task<Shop> ParseFromDto(ShopDto model);

    public Task<Ware> ParseFromDto(WareDto model);

    public Task<Trainer> ParseFromDto(TrainerDto trainer, IPokemonService pokemonService, IPokedexService pokedexService);

    public Task<User> ParseFromDto(UserDto model);

    public Task<UserMessageThread> ParseFromDto(UserMessageThreadDto dto);

    public Task<UserMessage> ParseFromDto(UserMessageDto dto);
}
