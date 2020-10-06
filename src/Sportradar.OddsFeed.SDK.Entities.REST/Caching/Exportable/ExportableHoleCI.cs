using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import hole item properties
    /// </summary>
    [Serializable]
    public class ExportableHoleCI
    {
        /// <summary>
        /// Gets the number of the hole
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets the par
        /// </summary>
        /// <value>The par</value>
        public int Par { get; set; }
    }
}
