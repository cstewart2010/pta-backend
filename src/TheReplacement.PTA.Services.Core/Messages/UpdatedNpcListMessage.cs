using System;
using System.Collections.Generic;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class UpdatedNpcListMessage : AbstractMessage
    {
        internal UpdatedNpcListMessage(IEnumerable<Guid> npcs)
        {
            Message = "Updated npc list";
            Npcs = npcs;
        }

        public override string Message { get; }
        public IEnumerable<Guid> Npcs { get; }
    }
}
