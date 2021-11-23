using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class FoundGameMessage : AbstractMessage
    {
        internal FoundGameMessage(string gameId)
        {
            Message = "Game was found";
            GameId = gameId;
            Trainers = DatabaseUtility.FindTrainersByGameId(gameId).Select(trainer => new StrippedTrainer(trainer));
        }

        public override string Message { get; }
        public string GameId { get; }
        public IEnumerable<StrippedTrainer> Trainers { get; }
    }
}
