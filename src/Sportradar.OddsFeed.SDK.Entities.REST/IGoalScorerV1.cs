/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing goal scorer in a sport event
    /// </summary>
    public interface IGoalScorerV1 : IGoalScorer
    {
        /// <summary>
        /// Gets the method value
        /// </summary>
        /// <value>The method value</value>
        /// <remarks>The attribute can assume values such as 'penalty' and 'own goal'. In case the attribute is not inserted, then the goal is not own goal neither penalty.</remarks>
        string Method { get; }
    }
}