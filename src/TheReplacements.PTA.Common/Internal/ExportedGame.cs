using System.Collections.Generic;
using System.Linq;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Utilities;

namespace TheReplacements.PTA.Common.Internal
{
    internal class ExportedGame
    {
        public ExportedGame() { }

        public ExportedGame(GameModel game)
        {
            GameSession = game;
            var trainers = DatabaseUtility.FindTrainersByGameId(game.GameId);
            Trainers = trainers.Select(trainer => new ExportedTrainer(trainer));
        }

        public GameModel GameSession { get; set; }
        public IEnumerable<ExportedTrainer> Trainers { get; set; }
    }
}
