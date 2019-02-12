/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A ReferenceId representation for cache
    /// </summary>
    public class ReferenceIdCI
    {
        /// <summary>
        /// A <see cref="BetradarId"/> property backing field
        /// </summary>
        private int _betradarId;

        /// <summary>
        /// A <see cref="BetfairId"/> property backing field
        /// </summary>
        private int _betfairId;

        /// <summary>
        /// The rotation number
        /// </summary>
        private int _rotationNumber;

        /// <summary>
        /// Gets the Betradar id for this instance if provided among reference ids
        /// </summary>
        /// <returns>If exists among reference ids, result is greater then 0</returns>
        public int BetradarId => _betradarId;

        /// <summary>
        /// Gets the Betfair id for this instance if provided among reference ids
        /// </summary>
        /// <returns>If exists among reference ids, result is greater then 0</returns>
        public int BetfairId => _betfairId;

        /// <summary>
        /// Gets the rotation number if provided among reference ids
        /// </summary>
        /// <value>If exists among reference ids, result is greater then 0</value>
        /// <remarks>This id only exists for US leagues</remarks>
        public int RotationNumber => _rotationNumber;

        /// <summary>
        /// Gets all the reference ids
        /// </summary>
        public IReadOnlyDictionary<string, string> ReferenceIds { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceIdCI"/> class
        /// </summary>
        public ReferenceIdCI(IDictionary<string, string> referenceIds)
        {
            _betradarId = 0;
            _betfairId = 0;
            _rotationNumber = 0;
            if (referenceIds == null)
            {
                return;
            }

            ReferenceIds = new ReadOnlyDictionary<string, string>(referenceIds);

            Merge(referenceIds, false);
        }

        /// <summary>
        /// Merges the specified reference ids
        /// </summary>
        /// <param name="referenceIds">The reference ids</param>
        /// <param name="checkDic">Check the dictionary for new values</param>
        public void Merge(IDictionary<string, string> referenceIds, bool checkDic)
        {
            if (referenceIds == null)
            {
                return;
            }
            string val;
            if (referenceIds.TryGetValue("betradar", out val))
            {
                int.TryParse(val, out _betradarId);
            }
            if (referenceIds.TryGetValue("BetradarCtrl", out val))
            {
                int.TryParse(val, out _betradarId);
            }
            if (referenceIds.TryGetValue("betfair", out val))
            {
                int.TryParse(val, out _betfairId);
            }
            if (referenceIds.TryGetValue("rotation_number", out val))
            {
                int.TryParse(val, out _rotationNumber);
            }

            if (checkDic)
            {
                var refIds = ReferenceIds == null
                                 ? new Dictionary<string, string>()
                                 : new Dictionary<string, string>(referenceIds);
                foreach (var id in referenceIds)
                {
                    if (refIds.ContainsKey(id.Key))
                    {
                        refIds[id.Key] = id.Value;
                    }
                    else
                    {
                        refIds.Add(id.Key, id.Value);
                    }
                }
                ReferenceIds = new ReadOnlyDictionary<string, string>(refIds);
            }
        }
    }
}
