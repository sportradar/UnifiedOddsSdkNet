﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
#pragma warning disable 1591

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    public class TeamStatisticsDTO
    {
        public URN TeamId { get; }

        public string Name { get; }

        public HomeAway? HomeOrAway { get; }

        public int? Cards { get; }

        public int? YellowCards { get; }

        public int? RedCards { get; }

        public int? YellowRedCards { get; }

        public int? CornerKicks { get; }

        public int? GreenCards { get; }
        
        internal TeamStatisticsDTO(string name, URN teamId, HomeAway? homeAway, int? yellowCards, int? redCards, int? yellowRedCards, int? cornerKicks, int? greenCards)
        {
            Name = name; // not available on the AMQP message
            TeamId = teamId; // not available on the AMQP message
            HomeOrAway = homeAway;
            YellowCards = yellowCards;
            RedCards = redCards;
            YellowRedCards = yellowRedCards;
            CornerKicks = cornerKicks;
            GreenCards = greenCards;
            var valueExists = false;
            var c = 0;
            if(yellowCards != null)
            {
                valueExists = true;
                c += yellowCards.Value;
            }
            if(redCards != null)
            {
                valueExists = true;
                c += redCards.Value;
            }
            if(yellowRedCards != null)
            {
                valueExists = true;
                c += yellowRedCards.Value;
            }
            if(greenCards != null)
            {
                valueExists = true;
                c += greenCards.Value;
            }
            Cards = valueExists ? c : (int?)null;
        }

        internal TeamStatisticsDTO(teamStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(statistics, nameof(statistics)).NotNull();

            Name = statistics.name;
            TeamId = !string.IsNullOrEmpty(statistics.id)
                ? URN.Parse(statistics.id)
                : null;

            HomeOrAway = null;
            if (TeamId != null && homeAwayCompetitors != null)
            {
                var x = homeAwayCompetitors.Where(w => w.Value.Equals(URN.Parse(statistics.id))).ToList();
                if (x.Any())
                {
                    HomeOrAway = x.First().Key == HomeAway.Home ? HomeAway.Home : HomeAway.Away;
                }
            }

            if (statistics.statistics != null)
            {
                if (int.TryParse(statistics.statistics.yellow_cards, out var tmp))
                {
                    YellowCards = tmp;
                }
                if (int.TryParse(statistics.statistics.red_cards, out tmp))
                {
                    RedCards = tmp;
                }
                if (int.TryParse(statistics.statistics.yellow_red_cards, out tmp))
                {
                    YellowRedCards = tmp;
                }
                if (int.TryParse(statistics.statistics.cards, out tmp))
                {
                    Cards = tmp;
                }
                if (int.TryParse(statistics.statistics.corner_kicks, out tmp))
                {
                    CornerKicks = tmp;
                }
            }
        }
    }
}
