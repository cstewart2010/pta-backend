using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common.Models
{
    public class StatModel
    {
        public int Base { get; set; }
        public int Nature { get; set; }
        public int Modifier { get; set; }
        public int Added { get; set; }
    }
}
