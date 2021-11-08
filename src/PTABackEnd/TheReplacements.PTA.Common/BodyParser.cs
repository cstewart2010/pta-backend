using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common
{
    public static class BodyParser
    {
        public static T FromBody<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
