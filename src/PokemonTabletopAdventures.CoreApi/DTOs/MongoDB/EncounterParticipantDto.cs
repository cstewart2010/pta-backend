using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Settings;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a participant to an encounter during a PTA session
/// </summary>
internal class SettingParticipantModel
{
    public SettingParticipantModel() { }
    /// <summary>
    /// Initiziles a new instance of <see cref="SettingParticipantModel"/>
    /// </summary>
    /// <param name="currentHp">The participant's current hp</param>
    /// <param name="totalHp">The participant's total hp</param>
    public SettingParticipantModel(double currentHp, double totalHp)
    {
        Health = GetHealth(currentHp, totalHp);
    }

    /// <summary>
    /// The paticipant's id
    /// </summary>
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// The paticipant's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The pariticipant's type (Trainer/Pokemon/Npc)
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public SettingParticipantType Type { get; set; }

    /// <summary>
    /// A description of the participant's health
    /// </summary>
    public string Health { get; set; } = string.Empty;

    /// <summary>
    /// The pariticipant's speed
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// The participants's position on the map
    /// </summary>
    public MapPosition Position { get; set; } = new();

    /// <summary>
    /// Returns the trainer as a participant
    /// </summary>
    /// <param name="trainerId"></param>
    /// <param name="gameId"></param>
    /// <param name="position"></param>
    public static SettingParticipantModel FromTrainer(TrainerDto trainer, MapPosition position)
    {
        return new SettingParticipantModel(trainer.CurrentHP, trainer.TrainerStats.HP)
        {
            ParticipantId = trainer.TrainerId,
            Name = trainer.TrainerName,
            Type = SettingParticipantType.Trainer,
            Speed = trainer.TrainerStats.Speed,
            Position = position
        };
    }

    /// <summary>
    /// Returns the Shop as a participant
    /// </summary>
    /// <param name="shopId"></param>
    /// <param name="gameId"></param>
    /// <param name="position"></param>
    public static SettingParticipantModel FromShop(ShopDto shop, MapPosition position)
    {
        return new SettingParticipantModel
        {
            ParticipantId = shop.ShopId,
            Name = shop.Name,
            Type = SettingParticipantType.Shop,
            Position = position
        };
    }

    /// <summary>
    /// Returns the pokemon as a participant
    /// </summary>
    /// <param name="pokemonId"></param>
    /// <param name="position"></param>
    /// <param name="type"></param>
    public static SettingParticipantModel FromPokemon(PokemonDto pokemon, MapPosition position, SettingParticipantType type)
    {
        return new SettingParticipantModel(pokemon.CurrentHP, pokemon.PokemonStats.HP)
        {
            ParticipantId = pokemon.PokemonId,
            Name = pokemon.Nickname,
            Type = type,
            Speed = pokemon.PokemonStats.Speed,
            Position = position
        };
    }

    /// <summary>
    /// Returns the npc as a participant
    /// </summary>
    /// <param name="npcId"></param>
    /// <param name="position"></param>
    /// <param name="type"></param>
    public static SettingParticipantModel FromNpc(NpcDto npc, MapPosition position, SettingParticipantType type)
    {
        return new SettingParticipantModel(npc.CurrentHP, npc.TrainerStats.HP)
        {
            ParticipantId = npc.NPCId,
            Name = npc.TrainerName,
            Type = type,
            Speed = npc.TrainerStats.Speed,
            Position = position
        };
    }

    private static string GetHealth(double currentHp, double totalHp)
    {
        var quotient = currentHp / totalHp;
        if (quotient >= 1)
        {
            return "Feeling fresh!";
        }
        else if (quotient > .6)
        {
            return "Going strong!";
        }
        else if (quotient > .3)
        {
            return "Might need some help. . .";
        }
        else if (quotient > 0)
        {
            return "Help!!!";
        }
        else if (quotient > -1)
        {
            return "Incapacitated";
        }

        return "";
    }
}
