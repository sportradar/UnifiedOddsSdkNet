/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Messages.Internal.REST
{
    /// <summary>
    /// A helper class providing an easier way of constructing instances which have only default constructor
    /// </summary>
    public static class RestMessageBuilder
    {
        public static seasonCoverageInfo BuildCoverageRecord(string maxCoverageLevel, string minCoverageLevel, int? maxCovered, int played, int scheduled, string seasonId)
        {
            var record = new seasonCoverageInfo
            {
                max_coverage_level = maxCoverageLevel,
                min_coverage_level = minCoverageLevel,
                played = played,
                scheduled = scheduled,
                season_id = seasonId
            };

            if (maxCovered != null)
            {
                record.max_covered = maxCovered.Value;
                record.max_coveredSpecified = true;
            }

            return record;
        }

        public static seasonExtended BuildSeasonExtendedRecord(string id, string name, DateTime startDate, DateTime endDate, string year)
        {
            return new seasonExtended
            {
                id = id,
                name = name,
                start_date = startDate,
                end_date = endDate,
                year = year
            };
        }

        public static bookmaker_details BuildBookmakerDetails(int? id, DateTime? expiresAt, response_code? responseCode, string virtualHost)
        {
            var record = new bookmaker_details
            {
                bookmaker_id = id ?? 0,
                bookmaker_idSpecified = id != null,
                expire_atSpecified = expiresAt != null,
                response_codeSpecified = responseCode != null,
                virtual_host = virtualHost
            };

            if (responseCode != null)
            {
                record.response_code = responseCode.Value;
            }
            if (expiresAt != null)
            {
                record.expire_at = expiresAt.Value;
            }

            return record;
        }
    }
}
