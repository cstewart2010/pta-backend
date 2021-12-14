using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class GameMasterMessage : AbstractMessage
    {
        internal GameMasterMessage(string gameMasterId)
        {
            Message = "Game was found";
            GameMasterId = gameMasterId;
            GameId = DatabaseUtility.FindTrainerById(gameMasterId).GameId;
            Trainers = DatabaseUtility.FindTrainersByGameId(GameId).Select(trainer => new PublicTrainer(trainer));
        }

        public override string Message { get; }
        public string GameId { get; }
        public string GameMasterId { get; }
        public IEnumerable<PublicTrainer> Trainers { get; }
    }
}
