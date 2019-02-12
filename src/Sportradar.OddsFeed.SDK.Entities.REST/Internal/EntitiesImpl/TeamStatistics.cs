/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    class TeamStatistics : ITeamStatistics
    {
        public HomeAway? HomeAway { get; }
        public int? Cards { get; }
        public int? YellowCards { get; }
        public int? RedCards { get; }
        public int? YellowRedCards { get; }
        public int? CornerKicks { get; }

        public TeamStatistics(TeamStatisticsDTO dto)
        {
            Contract.Requires(dto != null);

            HomeAway = dto.HomeOrAway;
            Cards = dto.Cards;
            YellowCards = dto.YellowCards;
            RedCards = dto.RedCards;
            YellowRedCards = dto.YellowRedCards;
            CornerKicks = dto.CornerKicks;
        }
    }
}
