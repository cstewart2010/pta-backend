using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Common.Internal
{
    internal class ExportedTrainer
    {
        public ExportedTrainer() { }

        public ExportedTrainer(TrainerModelv1 trainer)
        {
            trainer.IsOnline = false;
            Trainer = trainer;
            DatabaseUtility.UpdateTrainerOnlineStatus(trainer.TrainerId, false);
            Pokemon = DatabaseUtility.FindPokemonByTrainerId(trainer.TrainerId, trainer.GameId);
        }
        public TrainerModelv1 Trainer { get; set; }
        public IEnumerable<PokemonModelv1> Pokemon { get; set; }
    }
}
