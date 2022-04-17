using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a participant to an encounter during a PTA session
    /// </summary>
    public class EncounterParticipantModel
    {
        /// <summary>
        /// The paticipant's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The paticipant's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The paticipant's type (Trainer/Pokemon/Npc)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The paticipant's hp
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// The participants's position on the map
        /// </summary>
        public MapPositionModel Position { get; set; }

        /// <summary>
        /// Returns the trainer as a participant
        /// </summary>
        /// <param name="trainerId"></param>
        public static EncounterParticipantModel FromTrainer(string trainerId)
        {
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            return new EncounterParticipantModel
            {
                Id = trainer.TrainerId,
                Name = trainer.TrainerName,
                Type = "Trainer",
                HP = trainer.CurrentHP
            };
        }

        /// <summary>
        /// Returns the pokemon as a participant
        /// </summary>
        /// <param name="pokemonId"></param>
        public static EncounterParticipantModel FromPokemon(string pokemonId)
        {
            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            return new EncounterParticipantModel
            {
                Id = pokemon.PokemonId,
                Name = pokemon.Nickname,
                Type = "Pokemon",
                HP = pokemon.CurrentHP
            };
        }

        /// <summary>
        /// Returns the npc as a participant
        /// </summary>
        /// <param name="npcId"></param>
        public static EncounterParticipantModel FromNpc(string npcId)
        {
            var npc = DatabaseUtility.FindNpc(npcId);
            return new EncounterParticipantModel
            {
                Id = npc.NPCId,
                Name = npc.TrainerName,
                Type = "Trainer",
                HP = npc.CurrentHP
            };
        }
    }
}
