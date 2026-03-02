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
        return Shared.GameIds.Select((gameId, gameIndex) => Shared.SettingIds.Select((_, settingIndex) =>
        {
            var (x, y) = (0, 0);
            List<SettingParticipantType> types1 =
            [
                SettingParticipantType.Pokemon, SettingParticipantType.EnemyPokemon,
                SettingParticipantType.NeutralPokemon
            ];
            List<SettingParticipantType> types2 =
                [SettingParticipantType.NeutralNpc, SettingParticipantType.EnemyNpc];
            var trainerParticipants =
                trainerCollection.Collection.Where(z => z.GameId == gameId).Select(trainer => SettingParticipantModel.FromTrainer(trainer, new MapPosition
                {
                    X = x++,
                    Y = y++
                }));
            var pokemonParticipants = pokemonCollection.Collection.Where(z => z.GameId == gameId).Select(pokemon =>
                SettingParticipantModel.FromPokemon(
                    pokemon,
                    new MapPosition
                    {
                        X = x++,
                        Y = y++
                    },
                    types1[settingIndex % 3]));
            var npcParticipants = npcCollection.Collection.Where(z => z.GameId == gameId).Select(npc =>
                SettingParticipantModel.FromNpc(
                    npc,
                    new MapPosition
                    {
                        X = x++,
                        Y = y++
                    },
                    types2[settingIndex % 2]));
            var shopParticipants =
                shopCollection.Collection.Where(z => z.GameId == gameId).Select(shop => SettingParticipantModel.FromShop(shop, new MapPosition
                {
                    X = x++,
                    Y = y++
                }));
            return new SettingDto
            {
                ActiveParticipants =
                [
                    ..trainerParticipants,
                    ..pokemonParticipants,
                    ..npcParticipants,
                    ..shopParticipants
                ],
                Environment = [..Enumerable.Range(0, 3).Select(y => Guid.NewGuid().ToString())],
                GameId = gameId,
                IsActive = gameIndex == settingIndex,
                Name = y.ToString(),
                SettingId = Guid.NewGuid(),
                Shops = Shared.ShopIds,
                Type = (SettingType)Random.Shared.Next(1, 4)
            };
        })).SelectMany(x => x).ToList();
    }
}