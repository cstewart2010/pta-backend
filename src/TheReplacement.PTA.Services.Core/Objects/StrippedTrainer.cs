using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Services.Core.Objects
{
    public class StrippedTrainer
    {
        internal StrippedTrainer(TrainerModel trainer)
        {
            TrainerId = trainer.TrainerId;
            TrainerName = trainer.TrainerName;
            IsGM = trainer.IsGM;
            Items = trainer.Items;
            Feats = trainer.Feats;
            GameId = trainer.GameId;
            Honors = trainer.Honors;
            Money = trainer.Money;
            Origin = trainer.Origin;
            TrainerClasses = trainer.TrainerClasses;
            TrainerStats = trainer.TrainerStats;
            IsComplete = trainer.IsComplete;
        }

        public string TrainerId { get; }
        public string TrainerName { get; }
        public bool IsGM { get; }
        public IEnumerable<ItemModel> Items { get; }
        public IEnumerable<string> Feats { get; }
        public string GameId { get; }
        public IEnumerable<string> Honors { get; }
        public int Money { get; }
        public string Origin { get; }
        public IEnumerable<string> TrainerClasses { get; }
        public StatsModel TrainerStats { get; }
        public bool IsComplete { get; }
    }
}
