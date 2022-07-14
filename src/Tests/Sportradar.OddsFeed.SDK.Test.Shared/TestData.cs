﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public static class TestData
    {
        public static readonly string RestXmlPath = Directory.GetCurrentDirectory() + @"\REST XMLs\";
        public static readonly string FeedXmlPath = Directory.GetCurrentDirectory() + @"\XMLs\";

        public static readonly int BookmakerId = 1;
        public static readonly string AccessToken = "token";
        public static readonly string VirtualHost = "/virtualhost";

        public static readonly string DelayedUrl = "https://deelay.me/{0}/https://httpbin.org/get";

        public static readonly URN SportId = URN.Parse("sr:sport:1");
        public static readonly URN CategoryId = URN.Parse("sr:category:1");
        public static readonly URN TournamentId = URN.Parse("sr:tournament:1");
        public static readonly URN SimpleTournamentId = URN.Parse("sr:simple_tournament:1");
        public static readonly URN SeasonId = URN.Parse("sr:season:1");

        public static readonly URN EventId = URN.Parse("sr:match:9210275");
        public static readonly URN SimpleTournamentId11111 = URN.Parse("sr:simple_tournament:11111");

        public static readonly CultureInfo Culture = new CultureInfo("en");
        public static List<CultureInfo> Cultures => Cultures3;
        public static readonly List<CultureInfo> Cultures3 = new List<CultureInfo>(new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu") });
        public static readonly List<CultureInfo> Cultures4 = new List<CultureInfo>(new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu"), new CultureInfo("nl") });


        public static readonly CultureInfo CultureNl = new CultureInfo("nl");

        public const int CacheSportCount = 136;
        public const int CacheCategoryCount = 391;
        public const int CacheCategoryCountPlus = 408;
        public const int CacheTournamentCount = 8455;

        public const string SdkTestLogRepositoryName = "SdkTestLogRepositoryName";
        public const ExceptionHandlingStrategy ThrowingStrategy = ExceptionHandlingStrategy.THROW;

        public static void ValidateTestEventId(MatchCI ci, IEnumerable<CultureInfo> cultures, bool canHaveOtherLanguage)
        {
            Assert.IsNotNull(ci, "Cached item not found.");
            Assert.AreEqual(EventId, ci.Id);
            if (cultures == null)
            {
                Assert.IsNull(ci);
            }

            var date = new DateTime?();
            RoundCI round = null;
            SeasonCI season = null;

            var cultureInfos = cultures.ToList();
            var cStr = string.Join(",", cultureInfos);
            var lStr = string.Join(",", ci.LoadedSummaries);
            Assert.IsTrue(ci.HasTranslationsFor(cultureInfos), $"Culture missing: {cStr}, loaded summaries: {lStr}");
            if (!canHaveOtherLanguage)
            {
                Assert.IsTrue(ci.LoadedSummaries.Count >= cultureInfos.Count || ci.LoadedFixtures.Count >= cultureInfos.Count);
            }
            foreach (var culture in cultureInfos)
            {
                var checkCulture = new[] { culture };
                Task.Run(async () =>
                         {
                             date = await ci.GetScheduledAsync();
                             round = await ci.GetTournamentRoundAsync(checkCulture);
                             season = await ci.GetSeasonAsync(checkCulture);
                         }).GetAwaiter().GetResult();

                Debug.Assert(date != null, "date != null");
                Assert.AreEqual(new DateTime(2016, 08, 10), new DateTime(date.Value.Year, date.Value.Month, date.Value.Day));

                Assert.IsTrue(string.IsNullOrEmpty(round.GetName(culture)));

                Assert.IsTrue(Math.Max(ci.LoadedSummaries.Count, ci.LoadedFixtures.Count) >= season.Names.Count);
            }
        }

        public static void FillMessageTimestamp(FeedMessage message)
        {
            if (message != null)
            {
                var currentTimestamp = SdkInfo.ToEpochTime(DateTime.Now);

                message.SentAt = message.ReceivedAt = currentTimestamp;
            }
        }

        public static string GetDelayedUrl(TimeSpan delay)
        {
            return string.Format(DelayedUrl, delay.TotalMilliseconds);
        }

        public static string GetDelayedUrl(int delayMs)
        {
            return string.Format(DelayedUrl, delayMs);
        }
    }
}
