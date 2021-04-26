/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Runtime.Serialization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a player profile
    /// </summary>
    public interface IPlayerProfileV2 : IPlayerProfileV1
    {
        /// <summary>
        /// Gets the country code
        /// </summary>
        [DataMember]
        string CountryCode { get; }

        /// <summary>
        /// Gets the full name of the player
        /// </summary>
        [DataMember]
        string FullName { get; }

        /// <summary>
        /// Gets the nickname of the player
        /// </summary>
        [DataMember]
        string Nickname { get; }
    }
}