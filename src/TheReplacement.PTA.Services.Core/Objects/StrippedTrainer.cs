using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Services.Core.Objects
{
    public class PublicTrainer
    {
        internal static PublicTrainer FromJson(JToken json)
        {
            return json.ToObject<PublicTrainer>();
        }

        internal PublicTrainer() { }

        internal PublicTrainer(TrainerModel trainer)
        {
            TrainerId = trainer.TrainerId;
            TrainerName = trainer.TrainerName;
            IsGM = trainer.IsGM;
            IsOnline = trainer.IsOnline;
            Feats = trainer.Feats;
            GameId = trainer.GameId;
            Honors = trainer.Honors;
            Money = trainer.Money;
            Origin = trainer.Origin;
            TrainerClasses = trainer.TrainerClasses;
            TrainerStats = trainer.TrainerStats;
            IsComplete = trainer.IsComplete;
            var trainerPokemon = DatabaseUtility.FindPokemonByTrainerId(trainer.TrainerId);
            PokemonTeam = trainerPokemon
                .Where(pokemon => pokemon.IsOnActiveTeam);
            PokemonHome = trainerPokemon
                .Where(pokemon => !pokemon.IsOnActiveTeam);

            PokeDex = DatabaseUtility.GetTrainerPokeDex(TrainerId).OrderBy(item => item.DexNo);
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
            NewPokemon = Array.Empty<NewPokemon>();
        }

        internal TrainerModel ParseBackToModel()
        {
            var trainer = DatabaseUtility.FindTrainerById(TrainerId);
            trainer.TrainerName = TrainerName;
            trainer.IsGM = IsGM;
            trainer.Feats = Feats;
            trainer.Money = Money;
            trainer.Origin = Origin;
            trainer.TrainerClasses = TrainerClasses;
            trainer.TrainerStats = TrainerStats;
            trainer.TrainerSkills = TrainerSkills;
            trainer.Age = Age;
            trainer.Gender = Gender;
            trainer.Height = Height;
            trainer.Weight = Weight;
            trainer.Description = Description;
            trainer.Personality = Personality;
            trainer.Background = Background;
            trainer.Goals = Goals;
            trainer.Species = Species;
            return trainer;
        }

        public string TrainerId { get; set; }
        public string TrainerName { get; set; }
        public bool IsGM { get; set; }
        public bool IsOnline { get; set; }
        public IEnumerable<string> Feats { get; set; }
        public string GameId { get; set; }
        public IEnumerable<string> Honors { get; set; }
        public int Money { get; set; }
        public string Origin { get; set; }
        public IEnumerable<string> TrainerClasses { get; set; }
        public IEnumerable<PokemonModel> PokemonTeam { get; set; }
        public IEnumerable<PokemonModel> PokemonHome { get; set; }
        public IEnumerable<PokeDexItemModel> PokeDex { get; set; }
        public IEnumerable<NewPokemon> NewPokemon { get; set; }
        public StatsModel TrainerStats { get; set; }
        public bool IsComplete { get; set; }
        public int SeenTotal { get; set; }
        public int CaughtTotal { get; set; }
        public int Level { get; set; }
        public IEnumerable<TrainerSkill> TrainerSkills { get; set; }

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
