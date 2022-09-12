using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class FoundTrainerMessage : AbstractMessage
    {
        internal FoundTrainerMessage(string userId, string gameId)
        {
            var trainer = DatabaseUtility.FindTrainerById(userId, gameId);
            var user = DatabaseUtility.FindUserById(userId);
            Message = "Trainer was found";
            Trainer = new PublicTrainer(trainer);
            User = new PublicUser(user);
            GameNickname = DatabaseUtility.GetGameNickname(trainer.GameId);
        }

        public override string Message { get; }
        public PublicTrainer Trainer { get; }
        public PublicUser User { get; }
        public string GameNickname { get; set; }
    }
}
