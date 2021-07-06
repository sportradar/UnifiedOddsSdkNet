﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
#pragma warning disable 1591

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
        public EventPlayerCI GoalScorer;
        public EventPlayerCI Player;
        public int? MatchStatusCode;
        public string MatchClock;

        internal TimelineEventCI(BasicEventDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Id = dto.Id;
            Merge(dto, culture);
        }

        internal TimelineEventCI(ExportableTimelineEventCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            Id = exportable.Id;
            HomeScore = exportable.HomeScore;
            AwayScore = exportable.AwayScore;
            MatchTime = exportable.MatchTime;
            Period = exportable.Period;
            PeriodName = exportable.PeriodName;
            Points = exportable.Points;
            StoppageTime = exportable.StoppageTime;
            Team = exportable.Team;
            Type = exportable.Type;
            Value = exportable.Value;
            X = exportable.X;
            Y = exportable.Y;
            Time = exportable.Time;
            Assists = exportable.Assists?.Select(a => new EventPlayerAssistCI(a)).ToList();
            GoalScorer = exportable.GoalScorer != null ? new EventPlayerCI(exportable.GoalScorer) : null;
            Player = exportable.Player != null ? new EventPlayerCI(exportable.Player) : null;
            MatchStatusCode = exportable.MatchStatusCode;
            MatchClock = exportable.MatchClock;
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
                    GoalScorer = new EventPlayerCI(dto.GoalScorer, culture);
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
                    Player = new EventPlayerCI(dto.Player, culture);
                }
                else
                {
                    Player.Merge(dto.Player, culture);
                }
            }

            MatchStatusCode = dto.MatchStatusCode;
            MatchClock = dto.MatchClock;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableTimelineEventCI> ExportAsync()
        {
            var assistsTasks = Assists?.Select(async a => await a.ExportAsync().ConfigureAwait(false)).ToList();
            if (!assistsTasks.IsNullOrEmpty())
            {
                await Task.WhenAll(assistsTasks).ConfigureAwait(false);
            }

            return new ExportableTimelineEventCI
                   {
                       Id = Id,
                       Value = Value,
                       Type = Type,
                       GoalScorer = GoalScorer != null
                                        ? new ExportableEventPlayerCI
                                          {
                                              Id = GoalScorer.Id.ToString(),
                                              Name = new Dictionary<CultureInfo, string>(GoalScorer.Name ?? new Dictionary<CultureInfo, string>()),
                                              Bench = GoalScorer.Bench,
                                              Method =  GoalScorer.Method
                                          }
                                        : null,
                       MatchStatusCode = MatchStatusCode,
                       Assists = assistsTasks.IsNullOrEmpty() ? null : assistsTasks.Select(s=>s.Result).ToList(),
                       HomeScore = HomeScore,
                       Time = Time,
                       StoppageTime = StoppageTime,
                       Period = Period,
                       Points = Points,
                       MatchTime = MatchTime,
                       Team = Team,
                       PeriodName = PeriodName,
                       AwayScore = AwayScore,
                       MatchClock = MatchClock,
                       Y = Y,
                       X = X,
                       Player = Player != null
                                    ? new ExportableEventPlayerCI
                                      {
                                          Id = Player.Id.ToString(),
                                          Name = new Dictionary<CultureInfo, string>(Player.Name ?? new Dictionary<CultureInfo, string>()),
                                          Bench = Player.Bench,
                                          Method =  Player.Method
                                      }
                                    : null
                   };
        }
    }
}
