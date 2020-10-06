﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import pitcher cache item properties
    /// </summary>
    [Serializable]
    public class ExportableStageCI : ExportableCompetitionCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the category id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// A id representing the parent stage
        /// </summary>
        public URN ParentStageId { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the child stages
        /// </summary>
        public IEnumerable<ExportableStageCI> ChildStages { get; set; }

        /// <summary>
        /// <summary>
        /// A list of additional parents ids
        /// </summary>
        public IEnumerable<URN> AdditionalParentIds { get; set; }
    }
}
