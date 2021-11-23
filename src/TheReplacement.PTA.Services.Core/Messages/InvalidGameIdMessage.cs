using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class InvalidGameIdMessage : AbstractMessage
    {
        internal InvalidGameIdMessage(TrainerModel trainer)
        {
            Message = $"{trainer.TrainerId} had an invalid game id";
            GameId = trainer.GameId;
        }

        public override string Message { get; }
        public string GameId { get; }
    }
}
