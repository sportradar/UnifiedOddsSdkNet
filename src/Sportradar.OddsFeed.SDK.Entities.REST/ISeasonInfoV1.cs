/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing
    /// </summary>
    public interface ISeasonInfoV1 : ISeasonInfo
    {
        /// <summary>
        /// Gets the start date of the season represented by the current instance
        /// </summary>
        DateTime StartDate { get; }

        /// <summary>
        /// Gets the end date of the season represented by the current instance
        /// </summary>
        /// <value>The end date.</value>
        DateTime EndDate { get; }

        /// <summary>
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        string Year { get; }

        /// <summary>
        /// Gets the associated tournament identifier.
        /// </summary>
        /// <value>The associated tournament identifier.</value>
        URN TournamentId { get; }
    }
}
