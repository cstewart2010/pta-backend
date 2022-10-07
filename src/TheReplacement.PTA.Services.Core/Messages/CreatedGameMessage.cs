using System;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class CreatedGameMessage : AbstractMessage
    {
        internal CreatedGameMessage(TrainerModel trainer)
        {
            Message = "Game was created";
            GameId = trainer.GameId;
            GameMaster = new PublicTrainer(trainer);
        }

        public override string Message { get; }
        public Guid GameId { get; }
        public PublicTrainer GameMaster { get; }
    }
}
