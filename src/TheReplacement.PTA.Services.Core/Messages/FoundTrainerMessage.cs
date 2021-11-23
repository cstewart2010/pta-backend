using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class FoundTrainerMessage : AbstractMessage
    {
        internal FoundTrainerMessage(TrainerModel trainer)
        {
            Message = "Trainer was found";
            Trainer = new StrippedTrainer(trainer);
        }

        public override string Message { get; }
        public StrippedTrainer Trainer { get; }
    }
}
