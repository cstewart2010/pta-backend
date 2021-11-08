using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common.Models
{
    public class Stat
    {
        public byte Base { get; set; }
        public byte Modifier { get; set; }
        public byte Added { get; set; }
    }
}
