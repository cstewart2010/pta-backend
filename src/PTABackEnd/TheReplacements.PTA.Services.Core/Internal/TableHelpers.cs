using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheReplacements.PTA.Common.Databases;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Services.Core.Internal
{
    internal static class TableHelpers
    {
        public static TableHelper<Trainer> TrainerTable = new TableHelper<Trainer>(27017, "localhost");
        public static TableHelper<Game> GameTable = new TableHelper<Game>(27017, "localhost");
    }
}
