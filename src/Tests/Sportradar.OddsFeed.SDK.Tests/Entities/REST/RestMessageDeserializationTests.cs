/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST;

public class RestMessageDeserializationTests
{
    /// <summary>
    /// Class under test
    /// </summary>
    private static readonly IDeserializer<RestMessage> Deserializer = new Deserializer<RestMessage>();

    [Fact]
    public async Task FixtureIsDeserialized()
    {
        using var stream = FileHelper.GetResource("fixtures_en.xml");
        var fixture = Deserializer.Deserialize<fixturesEndpoint>(stream);
        Assert.NotNull(fixture);
    }

    [Fact]
    public async Task MatchSummaryIsDeserialized()
    {
        using var stream = FileHelper.GetResource("match_summary.xml");
        var matchSummary = Deserializer.Deserialize<matchSummaryEndpoint>(stream);
        Assert.NotNull(matchSummary);
    }

    [Fact]
    public async Task RaceSummaryIsDeserialized()
    {
        using var stream = FileHelper.GetResource("race_summary.xml");
        var stageSummary = Deserializer.Deserialize<stageSummaryEndpoint>(stream);
        Assert.NotNull(stageSummary);
    }

    [Fact]
    public async Task RaceSummaryGiroIsDeserialized()
    {
        using var stream = FileHelper.GetResource("race_summary_giro.xml");
        var stageSummary = Deserializer.Deserialize<stageSummaryEndpoint>(stream);
        Assert.NotNull(stageSummary);
    }

    [Fact]
    public async Task EventsAreDeserialized()
    {
        using var stream = FileHelper.GetResource("events.xml");
        var events = Deserializer.Deserialize<scheduleEndpoint>(stream);
        Assert.NotNull(events);
    }

    [Fact]
    public async Task LiveEventsAreDeserialized()
    {
        using var stream = FileHelper.GetResource("live_events.xml");
        var liveEvents = Deserializer.Deserialize<scheduleEndpoint>(stream);
        Assert.NotNull(liveEvents);
    }

    [Fact]
    public async Task TournamentScheduleIsDeserialized()
    {
        using var stream = FileHelper.GetResource("tournament_schedule_en.xml");
        var schedule = Deserializer.Deserialize<tournamentSchedule>(stream);
        Assert.NotNull(schedule);
    }

    [Fact]
    public async Task OngoingEventInfoIsDeserialized()
    {
        using var stream = FileHelper.GetResource("ongoing_event_info.xml");
        var timeline = Deserializer.Deserialize<matchTimelineEndpoint>(stream);
        Assert.NotNull(timeline);
    }

    [Fact]
    public async Task AllTournamentsAreDeserialized()
    {
        using var stream = FileHelper.GetResource("tournaments_en.xml");
        var tournaments = Deserializer.Deserialize<tournamentsEndpoint>(stream);
        Assert.NotNull(tournaments);
    }

    [Fact]
    public async Task TournamentInfoIsDeserialized()
    {
        using var stream = FileHelper.GetResource("tournament_info.xml");
        var tournamentInfo = Deserializer.Deserialize<tournamentInfoEndpoint>(stream);
        Assert.NotNull(tournamentInfo);
    }

    [Fact]
    public async Task PlayerProfileIsDeserialized()
    {
        using var stream = FileHelper.GetResource("player_1_en.xml");
        var playerProfile = Deserializer.Deserialize<playerProfileEndpoint>(stream);
        Assert.NotNull(playerProfile);
    }

    [Fact]
    public async Task CompetitorProfileIsDeserialized()
    {
        using var stream = FileHelper.GetResource("competitor_1_en.xml");
        var competitorProfile = Deserializer.Deserialize<competitorProfileEndpoint>(stream);
        Assert.NotNull(competitorProfile);
    }

    [Fact]
    public async Task ProducersIsDeserialized()
    {
        using var stream = FileHelper.GetResource("producers.xml");
        var result = Deserializer.Deserialize<producers>(stream);
        Assert.NotNull(result);
    }
}
