﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a hole of a golf course
    /// </summary>
    public interface IHole
    {
        /// <summary>
        /// Gets the number of the hole
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the par
        /// </summary>
        /// <value>The par</value>
        int Par { get; }
    }
}
