using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a Pokemon Tabletop Adventures log
    /// </summary>
    public class LogModel
    {
        /// <summary>
        /// The user that the log comes from
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The action being logged
        /// </summary>
        public string Action { get; set; }
    }
}
