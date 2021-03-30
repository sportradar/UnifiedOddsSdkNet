/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a team competing in a sport event
    /// </summary>
    public interface ICompetitorV6 : ICompetitorV5
    {
        /// <summary>
        /// Gets the short name
        /// </summary>
        /// <value>The short name</value>
        string ShortName { get; }
    }
}
