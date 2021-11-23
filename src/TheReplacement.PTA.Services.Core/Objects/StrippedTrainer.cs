using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        }

        public string TrainerId { get; }
        public string TrainerName { get; }
        public bool IsGM { get; }
        public IEnumerable<ItemModel> Items { get; }
    }
}
