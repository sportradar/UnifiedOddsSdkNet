﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import fixture cache item properties
    /// </summary>
    [Serializable]
    public class ExportableFixtureCI
    {
        /// <summary>
        /// A <see cref="DateTime"/> representation of the start time
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/> representation of the next live time
        /// </summary>
        public DateTime? NextLiveTime { get; set; }

        /// <summary>
        /// A <see cref="bool"/> indicating if the start time is confirmed
        /// </summary>
        public bool? StartTimeConfirmed { get; set; }

        /// <summary>
        /// A <see cref="bool"/> indicating if the start time is TBD
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool? StartTimeTBD { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the replaced by
        /// </summary>
        public string ReplacedBy { get; set; }

        /// <summary>
        /// A <see cref="Dictionary{K, V}"/> representation of the extra info
        /// </summary>
        public Dictionary<string, string> ExtraInfo { get; set; }

        /// <summary>
        /// A <see cref="List{T}"/> representation of the tv channels
        /// </summary>
        public List<ExportableTvChannelCI> TvChannels { get; set; }

        /// <summary>
        /// A <see cref="ExportableCoverageInfoCI"/> representation of the coverage info
        /// </summary>
        public ExportableCoverageInfoCI CoverageInfo { get; set; }

        /// <summary>
        /// A <see cref="ExportableProductInfoCI"/> representation of the product info
        /// </summary>
        public ExportableProductInfoCI ProductInfo { get; set; }

        /// <summary>
        /// A <see cref="Dictionary{K, V}"/> representation of the references
        /// </summary>
        public Dictionary<string, string> References { get; set; }

        /// <summary>
        /// A <see cref="List{T}"/> representation of the scheduled start time changes
        /// </summary>
        public List<ExportableScheduledStartTimeChangeCI> ScheduledStartTimeChanges { get; set; }

        /// <summary>
        /// Gets a id of the parent stage associated with the current instance
        /// </summary>
        public string ParentStageId { get; set; }

        /// <summary>
        /// Gets a <see cref="List{T}"/> specifying the additional parent stages associated with the current instance
        /// </summary>
        public List<string> AdditionalParentsIds { get; set; }
    }
}
