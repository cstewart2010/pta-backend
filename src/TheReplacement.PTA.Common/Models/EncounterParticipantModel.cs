using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a participant to an encounter during a PTA session
    /// </summary>
    public class EncounterParticipantModel
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EncounterParticipantModel() { }

        /// <summary>
        /// Initiziles a new instance of <see cref="EncounterParticipantModel"/>
        /// </summary>
        /// <param name="currentHp">The participant's current hp</param>
        /// <param name="totalHp">The participant's total hp</param>
        public EncounterParticipantModel(double currentHp, double totalHp)
        {
            Health = GetHealth(currentHp, totalHp);
        }

        /// <summary>
        /// The paticipant's id
        /// </summary>
        public string ParticipantId { get; set; }

        /// <summary>
        /// The paticipant's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The pariticipant's type (Trainer/Pokemon/Npc)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A description of the participant's health
        /// </summary>
        public string Health { get; set; }

        /// <summary>
        /// The pariticipant's speed
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// The participants's position on the map
        /// </summary>
        public MapPositionModel Position { get; set; }

        /// <summary>
        /// Returns the trainer as a participant
        /// </summary>
        /// <param name="trainerId"></param>
        /// <param name="position"></param>
        public static EncounterParticipantModel FromTrainer(string trainerId, MapPositionModel position)
        {
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            return new EncounterParticipantModel(trainer.CurrentHP, trainer.TrainerStats.HP)
            {
                ParticipantId = trainer.TrainerId,
                Name = trainer.TrainerName,
                Type = "Trainer",
                Speed = trainer.TrainerStats.Speed,
                Position = position
            };
        }

        /// <summary>
        /// Returns the pokemon as a participant
        /// </summary>
        /// <param name="pokemonId"></param>
        /// <param name="position"></param>
        public static EncounterParticipantModel FromPokemon(string pokemonId, MapPositionModel position)
        {
            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            return new EncounterParticipantModel(pokemon.CurrentHP, pokemon.PokemonStats.HP)
            {
                ParticipantId = pokemon.PokemonId,
                Name = pokemon.Nickname,
                Type = "Pokemon",
                Speed = pokemon.PokemonStats.Speed,
                Position = position
            };
        }

        /// <summary>
        /// Returns the npc as a participant
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="position"></param>
        public static EncounterParticipantModel FromNpc(string npcId, MapPositionModel position)
        {
            var npc = DatabaseUtility.FindNpc(npcId);
            return new EncounterParticipantModel(npc.CurrentHP, npc.TrainerStats.HP)
            {
                ParticipantId = npc.NPCId,
                Name = npc.TrainerName,
                Type = "Trainer",
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
}
