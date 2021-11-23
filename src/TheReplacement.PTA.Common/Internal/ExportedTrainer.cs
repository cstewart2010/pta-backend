using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Common.Internal
{
    internal class ExportedTrainer
    {
        public ExportedTrainer() { }

        public ExportedTrainer(TrainerModel trainer)
        {
            trainer.IsOnline = false;
            Trainer = trainer;
            DatabaseUtility.UpdateTrainerOnlineStatus(trainer.TrainerId, false);
            Pokemon = DatabaseUtility.FindPokemonByTrainerId(trainer.TrainerId);
        }
        public TrainerModel Trainer { get; set; }
        public IEnumerable<PokemonModel> Pokemon { get; set; }
    }
}
