﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Xml.Serialization;
// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.Messages.Feed
{
    /// <summary>
    /// Base FeedMarket class
    /// </summary>
    public abstract class FeedMarket
    {
        /// <summary>
        /// Gets or sets a <see cref="IReadOnlyDictionary{String, String}"/> representing parsed specifiers
        /// </summary>
        [XmlIgnore]
        public IReadOnlyDictionary<string, string> Specifiers { get; set; }

        /// <summary>
        /// Gets or sets a indicating whether the validation of the market has failed and should not be mapped to exposed entity
        /// </summary>
        public bool ValidationFailed { get; set; }

        /// <summary>
        /// Gets the specifiers string
        /// </summary>
        [XmlIgnore]
        public abstract string SpecifierString { get; }
    }

    public partial class market : FeedMarket
    {
        /// <inheritdoc />
        public override string SpecifierString => specifiers;
    }

    public partial class betSettlementMarket : FeedMarket
    {
        /// <inheritdoc />
        public override string SpecifierString => specifiers;
    }

    public partial class oddsChangeMarket : FeedMarket
    {
        /// <inheritdoc />
        public override string SpecifierString => specifiers;
    }
}
