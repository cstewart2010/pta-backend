using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class FoundUserMessage : AbstractMessage
    {
        internal FoundUserMessage(UserModel user)
        {
            Message = "Trainer was found";
            User = new PublicUser(user);
        }

        public override string Message { get; }
        public PublicUser User { get; }
    }

}
