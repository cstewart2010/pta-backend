using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class FoundTrainerMessage : AbstractMessage
    {
        internal FoundTrainerMessage(TrainerModel trainer)
        {
            Message = "Trainer was found";
            Trainer = new StrippedTrainer(trainer);
            GameNickname = DatabaseUtility.GetGameNickname(trainer.GameId);
        }

        public override string Message { get; }
        public StrippedTrainer Trainer { get; }
        public string GameNickname { get; set; }
    }
}
