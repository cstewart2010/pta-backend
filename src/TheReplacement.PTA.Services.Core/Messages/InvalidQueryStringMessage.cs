using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class InvalidQueryStringMessage : AbstractMessage
    {
        public InvalidQueryStringMessage()
        {
            Message = "Missing the following parameters in the query";
        }

        public override string Message { get; }
        public IEnumerable<string> MissingParameters { get; set; }
        public IEnumerable<string> InvalidParameters { get; set; }
    }
}
