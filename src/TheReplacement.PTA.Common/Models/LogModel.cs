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
    public class LogModelv1
    {
        private LogModelv1() { }

        /// <summary>
        /// Initializes a new instance of <see cref="LogModelv1"/>
        /// </summary>
        public LogModelv1(string user, string action)
        {
            User = user;
            Action = action;
            LogTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// The user that the log comes from
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The action being logged
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The timestamp for the Log
        /// </summary>
        public DateTime? LogTimestamp { get; set; }
    }
}
