using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheReplacement.PTA.Services.Core.Messages
{
    public class StaticCollectionMessage
    {
        internal StaticCollectionMessage(
            int count,
            string previousUrl,
            string nextUrl,
            IEnumerable<ResultMessage> results)
        {
            Count = count;
            Previous = previousUrl;
            Next = nextUrl;
            Results = results;
        }

        public int Count { get; }
        public string Previous { get; }
        public string Next { get; }
        public IEnumerable<ResultMessage> Results { get; }
    }
}
