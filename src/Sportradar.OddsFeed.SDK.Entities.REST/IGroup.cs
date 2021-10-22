﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a competition group
    /// </summary>
    public interface IGroup : IEntityPrinter
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{ICompetitor}"/> representing group competitors
        /// </summary>
        IEnumerable<ICompetitor> Competitors { get; }
    }
}
