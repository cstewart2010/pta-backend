using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Services.Core.Objects
{
    public class PublicTrainer
    {
        internal PublicTrainer(TrainerModel trainer)
        {
            TrainerId = trainer.TrainerId;
            TrainerName = trainer.TrainerName;
            IsGM = trainer.IsGM;
            IsOnline = trainer.IsOnline;
            Items = trainer.Items;
            Feats = trainer.Feats;
            GameId = trainer.GameId;
            Honors = trainer.Honors;
            Money = trainer.Money;
            Origin = trainer.Origin;
            TrainerClasses = trainer.TrainerClasses;
            TrainerStats = trainer.TrainerStats;
            IsComplete = trainer.IsComplete;
            Pokemon = DatabaseUtility.FindPokemonByTrainerId(TrainerId);
            PokeDex = DatabaseUtility.GetTrainerPokeDex(TrainerId);
            SeenTotal = PokeDex.Count(dexItem => dexItem.IsSeen);
            CaughtTotal = PokeDex.Count(dexItem => dexItem.IsCaught);
            Level = Honors.Count() + CaughtTotal / 30 + 1;
            TrainerSkills = trainer.TrainerSkills;
            Age = trainer.Age;
            Gender = trainer.Gender;
            Height = trainer.Height;
            Weight = trainer.Weight;
            Description = trainer.Description;
            Personality = trainer.Personality;
            Background = trainer.Background;
            Goals = trainer.Goals;
            Species = trainer.Species;
        }

        public string TrainerId { get; }
        public string TrainerName { get; }
        public bool IsGM { get; }
        public bool IsOnline { get; }
        public IEnumerable<ItemModel> Items { get; }
        public IEnumerable<string> Feats { get; }
        public string GameId { get; }
        public IEnumerable<string> Honors { get; }
        public int Money { get; }
        public string Origin { get; }
        public IEnumerable<string> TrainerClasses { get; }
        public StatsModel TrainerStats { get; }
        public bool IsComplete { get; }
        public IEnumerable<PokemonModel> Pokemon { get; }
        public IEnumerable<PokeDexItemModel> PokeDex { get; }
        public int SeenTotal { get; }
        public int CaughtTotal { get; }
        public int Level { get; }
        public IEnumerable<TrainerSkill> TrainerSkills { get; }

        public int Age { get; set; }

        public string Gender { get; set; }

        public int Height { get; set; }

        public int Weight { get; set; }

        public string Description { get; set; }

        public string Personality { get; set; }

        public string Background { get; set; }

        public string Goals { get; set; }

        public string Species { get; set; }
    }
}
