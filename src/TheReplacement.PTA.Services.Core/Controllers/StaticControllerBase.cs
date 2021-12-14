using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Interfaces;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    public abstract class StaticControllerBase : ControllerBase
    {
        internal  string HostUrl => $"{Request.Scheme}://{Request.Host}{Request.Path}".Trim('/');

        public StaticCollectionMessage GetStaticCollectionResponse<TDocument>(IEnumerable<TDocument> documents) where TDocument : IDexDocument
        {
            if (!int.TryParse(Request.Query["offset"], out var offset))
            {
                offset = 0;
            }
            if (!int.TryParse(Request.Query["limit"], out var limit))
            {
                limit = 20;
            }

            var count = documents.Count();
            var previous = GetPreviousUrl(offset, limit);
            var next = GetNextUrl(offset, limit, count);
            var results = documents.GetSubset(offset, limit)
                .Select(GetResultsMember);

            return new StaticCollectionMessage
            (
                count,
                previous,
                next,
                results
            );
        }

        private string GetPreviousUrl(int offset, int limit)
        {
            int previousOffset = Math.Max(0, offset - limit);
            int previousLimit = offset - limit < 0 ? offset : limit;
            if (previousOffset == offset)
            {
                return null;
            }

            return $"{HostUrl}?offset={previousOffset}&limit={previousLimit}";
        }

        private string GetNextUrl(int offset, int limit, int count)
        {
            int nextOffset = Math.Min(offset + limit, count - limit);
            if (nextOffset <= offset)
            {
                return null;
            }

            return $"{HostUrl}?offset={nextOffset}&limit={limit}";
        }

        private ResultMessage GetResultsMember<TDocument>(TDocument document) where TDocument : IDexDocument
        {
            if (document == null)
            {
                return null;
            }

            return new ResultMessage
            {
                Name = document.Name,
                Url = $"{HostUrl}/{document.Name.Replace("/", "_")}"
            };
        }
    }
}
