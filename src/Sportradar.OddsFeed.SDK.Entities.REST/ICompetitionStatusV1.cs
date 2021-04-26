/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing the status of a <see cref="ICompetition"/>
    /// </summary>
    public interface ICompetitionStatusV1 : ICompetitionStatus
    {
        /// <summary>
        /// Gets the period of ladder
        /// </summary>
        /// <value>The period of ladder</value>
        int? PeriodOfLadder { get; }
    }
}
