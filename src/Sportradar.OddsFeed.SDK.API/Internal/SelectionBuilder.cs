/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// The run-time implementation of the <see cref="ISelectionBuilder"/> interface
    /// </summary>
    internal class SelectionBuilder : ISelectionBuilder
    {
        private URN _eventId;
        private int _marketId;
        private string _specifiers;
        private string _outcomeId;

        public ISelectionBuilder SetEventId(URN eventId)
        {
            _eventId = eventId;
            return this;
        }

        public ISelectionBuilder SetMarketId(int marketId)
        {
            _marketId = marketId;
            return this;
        }

        public ISelectionBuilder SetSpecifiers(string specifiers)
        {
            _specifiers = specifiers;
            return this;
        }

        public ISelectionBuilder SetOutcomeId(string outcomeId)
        {
            _outcomeId = outcomeId;
            return this;
        }

        public ISelection Build()
        {
            var selection = new Selection(_eventId, _marketId, _specifiers, _outcomeId);
            _eventId = null;
            _marketId = 0;
            _specifiers = null;
            _outcomeId = null;
            return selection;
        }

        public ISelection Build(URN eventId, int marketId, string specifiers, string outcomeId)
        {
            _eventId = eventId;
            _marketId = marketId;
            _specifiers = specifiers;
            _outcomeId = outcomeId;
            return Build();
        }
    }
}
