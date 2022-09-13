using System;
using TheReplacement.PTA.Common.Enums;
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
            IsAdmin = Enum.TryParse<UserRoleOnSite>(user.SiteRole, out var result) && result == UserRoleOnSite.SiteAdmin;
        }

        public override string Message { get; }
        public PublicUser User { get; }
        public bool IsAdmin { get; }
    }

}
