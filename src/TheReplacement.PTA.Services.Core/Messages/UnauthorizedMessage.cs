namespace TheReplacement.PTA.Services.Core.Messages
{
    public class UnauthorizedMessage : AbstractMessage
    {
        internal UnauthorizedMessage(string gameId)
        {
            Message = "Could not login in to game with provided password";
            GameId = gameId;
        }

        public override string Message { get; }
        public string GameId { get; }
    }
}
