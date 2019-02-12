/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// Class used to handle messages dispatched by non-specific entity dispatchers - the event is always represented as <see cref="ISportEvent"/>
    /// </summary>
    internal class EntityProcessor : IEntityProcessor
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for logging
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Log = LogManager.GetLogger(typeof(EntityProcessor));

        /// <summary>
        /// A <see cref="IEntityDispatcher{ISportEvent}"/> used to obtain SDK messages
        /// </summary>
        private readonly IEntityDispatcher<ISportEvent> _dispatcher;

        /// <summary>
        /// A <see cref="SportEntityWriter"/> used to write the sport entities data
        /// </summary>
        private readonly SportEntityWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityProcessor"/> class.
        /// </summary>
        /// <param name="dispatcher">A <see cref="IEntityDispatcher{ISportEvent}" /> whose dispatched entities will be processed by the current instance</param>
        /// <param name="writer">A <see cref="SportEntityWriter" /> used to output / log the dispatched entities</param>
        public EntityProcessor(IEntityDispatcher<ISportEvent> dispatcher, SportEntityWriter writer = null)
        {
            Contract.Requires(dispatcher != null);

            _dispatcher = dispatcher;
            _writer = writer;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Log != null);
            Contract.Invariant(_dispatcher != null);
            Contract.Invariant(_writer != null);
        }

        /// <summary>
        /// Invoked when bet stop message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetStopReceived(object sender, BetStopEventArgs<ISportEvent> e)
        {
            // this method should never be invoked because the entity is always processed by a specific entity dispatcher
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when odds change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnOddsChangeReceived(object sender, OddsChangeEventArgs<ISportEvent> e)
        {
            // this method should never be invoked because the entity is always processed by a specific entity dispatcher
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetSettlementReceived(object sender, BetSettlementEventArgs<ISportEvent> e)
        {
            // this method should never be invoked because the entity is always processed by a specific entity dispatcher
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when rollback bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> e)
        {
            // this method should never be invoked because the entity is always processed by a specific entity dispatcher
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetCancel(object sender, BetCancelEventArgs<ISportEvent> e)
        {
            // this method should never be invoked because the entity is always processed by a specific entity dispatcher
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when rollback bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> e)
        {
            // this method should never be invoked because the entity is always processed by a specific entity dispatcher
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when fixture change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> e)
        {
            // this method should never be invoked because the entity is always processed by a specific entity dispatcher
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens the current processor so it will start processing dispatched entities.
        /// </summary>
        public void Open()
        {
            Log.Info("Attaching to session events");
            _dispatcher.OnOddsChange += OnOddsChangeReceived;
            _dispatcher.OnBetCancel += OnBetCancel;
            _dispatcher.OnRollbackBetCancel += OnRollbackBetCancel;
            _dispatcher.OnBetStop += OnBetStopReceived;
            _dispatcher.OnBetSettlement += OnBetSettlementReceived;
            _dispatcher.OnRollbackBetSettlement += OnRollbackBetSettlement;
            _dispatcher.OnFixtureChange += OnFixtureChange;
        }

        /// <summary>
        /// Closes the current processor so it will no longer process dispatched entities
        /// </summary>
        public void Close()
        {
            Log.Info("Detaching from session events");
            _dispatcher.OnOddsChange -= OnOddsChangeReceived;
            _dispatcher.OnBetCancel -= OnBetCancel;
            _dispatcher.OnRollbackBetCancel -= OnRollbackBetCancel;
            _dispatcher.OnBetStop -= OnBetStopReceived;
            _dispatcher.OnBetSettlement -= OnBetSettlementReceived;
            _dispatcher.OnRollbackBetSettlement -= OnRollbackBetSettlement;
            _dispatcher.OnFixtureChange -= OnFixtureChange;
        }
    }
}
