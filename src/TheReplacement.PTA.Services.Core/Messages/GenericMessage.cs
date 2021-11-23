namespace TheReplacement.PTA.Services.Core.Messages
{
    public class GenericMessage : AbstractMessage
    {
        internal GenericMessage(string message)
        {
            Message = message;
        }

        public override string Message { get; }
    }
}
