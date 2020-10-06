﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines contract used by classes that provide competitor result information
    /// </summary>
    public interface ICompetitorResult
    {
        /// <summary>
        /// Get the type
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <value>The value</value>
        string Value { get; }

        /// <summary>
        /// Gets the specifiers
        /// </summary>
        /// <value>The specifiers</value>
        string Specifiers { get; }
    }
}
