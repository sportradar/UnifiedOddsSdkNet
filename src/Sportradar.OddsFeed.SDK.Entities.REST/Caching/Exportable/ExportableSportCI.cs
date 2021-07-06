﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import sport cache item properties
    /// </summary>
    [Serializable]
    public class ExportableSportCI : ExportableCI
    {
        /// <summary>
        /// A <see cref="List{T}"/> specifying the id's of child categories
        /// </summary>
        /// 
        public List<string> CategoryIds { get; set; }

        /// <summary>
        /// A <see cref="List{T}"/> specifying the loaded categories for tournament
        /// </summary>
        public List<CultureInfo> LoadedCategories { get; set; }
    }
}
