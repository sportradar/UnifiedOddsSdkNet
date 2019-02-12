/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for basic event (used in match timeline) (on REST and DTO is called basicEvent); this has different name to be similar to java version
    /// </summary>
    public class TimelineEventCI
    {
        public int Id;
        public decimal? HomeScore;
        public decimal? AwayScore;
        public int? MatchTime;
        public string Period;
        public string PeriodName;
        public string Points;
        public string StoppageTime;
        public HomeAway? Team;
        public string Type;
        public string Value;
        public int? X;
        public int? Y;
        public DateTime Time;
        public IEnumerable<EventPlayerAssistCI> Assists;
        public CacheItem GoalScorer;
        public CacheItem Player;
        public int? MatchStatusCode;
        public string MatchClock;

        internal TimelineEventCI(BasicEventDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);

            Id = dto.Id;
            Merge(dto, culture);
        }

        public void Merge(BasicEventDTO dto, CultureInfo culture)
        {
            HomeScore = dto.HomeScore;
            AwayScore = dto.AwayScore;
            MatchTime = dto.MatchTime;
            Period = dto.Period;
            PeriodName = dto.PeriodName;
            Points = dto.Points;
            StoppageTime = dto.StoppageTime;
            Team = dto.Team;
            Type = dto.Type;
            Value = dto.Value;
            X = dto.X;
            Y = dto.Y;
            Time = dto.Time;
            if (dto.Assists != null && dto.Assists.Any())
            {
                if (Assists == null || !Assists.Any())
                {
                    Assists = dto.Assists.Select(s => new EventPlayerAssistCI(s, culture));
                }
                else
                {
                    var newAssists = new List<EventPlayerAssistCI>();
                    foreach (var assist in dto.Assists)
                    {
                        var a = Assists.FirstOrDefault(f => Equals(f.Id, assist.Id));
                        if (a != null && a.Id.Equals(assist.Id))
                        {
                            a.Merge(assist, culture);
                            newAssists.Add(a);
                        }
                        else
                        {
                            newAssists.Add(new EventPlayerAssistCI(assist, culture));
                        }
                    }
                    Assists = newAssists;
                }
            }
            if (dto.GoalScorer != null)
            {
                if (GoalScorer == null)
                {
                    GoalScorer = new CacheItem(dto.GoalScorer.Id, dto.GoalScorer.Name, culture);
                }
                else
                {
                    GoalScorer.Merge(dto.GoalScorer, culture);
                }
            }
            if (dto.Player != null)
            {
                if (Player == null)
                {
                    Player = new CacheItem(dto.Player.Id, dto.Player.Name, culture);
                }
                else
                {
                    Player.Merge(dto.Player, culture);
                }
            }
            MatchStatusCode = dto.MatchStatusCode;
            MatchClock = dto.MatchClock;
        }
    }
}
