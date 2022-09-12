using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class GameMasterMessage : AbstractMessage
    {
        internal GameMasterMessage(string userId, string gameId)
        {
            var user = DatabaseUtility.FindUserById(userId);
            User = new PublicUser(user);
            Message = "Game was found";
            GameMasterId = userId;
            GameId = gameId;
            Trainers = DatabaseUtility.FindTrainersByGameId(GameId).Select(trainer => new PublicTrainer(trainer));
        }

        public override string Message { get; }
        public string GameId { get; }
        public string GameMasterId { get; }
        public IEnumerable<PublicTrainer> Trainers { get; }
        public PublicUser User { get; }
    }
}
