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
        /// Returns the trainer as a participant
        /// </summary>
        /// <param name="trainer"></param>
        public static EncounterParticipantModel FromTrainer(TrainerModel trainer)
        {
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
        /// <param name="pokemon"></param>
        public static EncounterParticipantModel FromPokemon(PokemonModel pokemon)
        {
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
        /// <param name="npc"></param>
        public static EncounterParticipantModel FromNpc(NpcModel npc)
        {
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
