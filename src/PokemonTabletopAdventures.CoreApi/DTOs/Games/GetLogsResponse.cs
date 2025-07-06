using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTabletopAdventures.CoreApi.DTOs.Games;

public class GetLogsResponse
{
    public GetLogsResponse() { }

    internal GetLogsResponse(GameModel game, int count)
    {
        if (count > game.Logs.Count)
        {
            count = game.Logs.Count;
        }

        LogPages = [];
        if (game.Logs == null)
        {
            return;
        }

        var logs = game.Logs.OrderByDescending(log => log.LogTimestamp);
        for (int i = 0; i < count; i += 50)
        {
            LogPages.Add(GetPage(logs, i, 50));
        }
    }

    private static IEnumerable<LogModel> GetPage(
        IEnumerable<LogModel> source,
        int offset,
        int limit)
    {
        if (offset < 0 || offset >= source.Count() || limit < 0)
        {
            return [];
        }

        if (limit >= source.Count())
        {
            return source.Skip(offset);
        }

        if (offset + limit > source.Count())
        {
            limit = source.Count() - offset;
        }

        return source.Skip(offset).Take(limit);
    }

    public ICollection<IEnumerable<LogModel>> LogPages { get; set; } = [];
}
