using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Settings;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class SettingCollectionImpl(
    TrainerCollectionImpl trainerCollection,
    PokemonCollectionImpl pokemonCollection,
    NpcCollectionImpl npcCollection,
    ShopCollectionImpl shopCollection) : BaseCollectionImpl<SettingDto>
{
    public override ICollection<SettingDto> Collection { get; protected set; } = InitializeCollection(
        trainerCollection,
        pokemonCollection,
        npcCollection,
        shopCollection);

    private static ICollection<SettingDto> InitializeCollection(
        TrainerCollectionImpl trainerCollection,
        PokemonCollectionImpl pokemonCollection,
        NpcCollectionImpl npcCollection,
        ShopCollectionImpl shopCollection)
    {
        var trainerParticipants =
            trainerCollection.Collection.Select(x => SettingParticipantModel.FromTrainer(x, new MapPosition()));
        var pokemonParticipants = pokemonCollection.Collection.Select(x =>
            SettingParticipantModel.FromPokemon(x, new MapPosition(), SettingParticipantType.Pokemon));
        var npcParticipants = npcCollection.Collection.Select(x =>
            SettingParticipantModel.FromNpc(x, new MapPosition(), SettingParticipantType.NeutralNpc));
        var shopParticipants =
            shopCollection.Collection.Select(x => SettingParticipantModel.FromShop(x, new MapPosition()));
        return Shared.GameIds.Select((x, gameIndex) => Shared.SettingIds.Select((y, settingIndex) =>new SettingDto
        {
            ActiveParticipants =
            [
                ..trainerParticipants,
                ..pokemonParticipants,
                ..npcParticipants,
                ..shopParticipants
            ],
            Environment = [..Enumerable.Range(0,3).Select(y => Guid.NewGuid().ToString())],
            GameId = x,
            IsActive = gameIndex == settingIndex,
            Name = y.ToString(),
            SettingId = y,
            Shops = Shared.ShopIds,
            Type = (SettingType)Random.Shared.Next(1, 4)
        })).SelectMany(x => x).ToList();
    }
}