using System;

namespace Sportradar.OddsFeed.SDK.Test.Messages
{
    /// <summary>
    /// Represents a base class for messages received from the feed
    /// </summary>
    public abstract class TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "alive", IgnoreNamespace = false)]
    public partial class alive : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "snapshot_complete", IgnoreNamespace = false)]
    public partial class snapshot_complete : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "odds_change", IgnoreNamespace = false)]
    public partial class odds_change : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "bet_stop", IgnoreNamespace = false)]
    public partial class bet_stop : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "bet_settlement", IgnoreNamespace = false)]
    public partial class bet_settlement : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "rollback_bet_settlement", IgnoreNamespace = false)]
    public partial class rollback_bet_settlement : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "bet_cancel", IgnoreNamespace = false)]
    public partial class bet_cancel : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "rollback_bet_cancel", IgnoreNamespace = false)]
    public partial class rollback_bet_cancel : TestFeedMessage
    {
    }

    [OverrideXmlNamespace(RootElementName = "fixture_change", IgnoreNamespace = false)]
    public partial class fixture_change : TestFeedMessage
    {
    }

    /// <summary>
    /// Attributes providing additional information to deserializers. This is only required to overcome problems
    /// caused by XSD schema issues and will be removed when the schemes are fixed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OverrideXmlNamespaceAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the xml namespace should be ignored when deserializing the
        /// xml message
        /// </summary>
        public bool IgnoreNamespace
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the document namespace.
        /// </summary>
        /// <value>The document namespace.</value>
        public string DocumentNamespace { get; set; }

        /// <summary>
        /// Gets or sets the name of the root xml element in the associated xml messages
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverrideXmlNamespaceAttribute"/> class
        /// </summary>
        public OverrideXmlNamespaceAttribute()
        {
            IgnoreNamespace = true;
        }
    }
}
