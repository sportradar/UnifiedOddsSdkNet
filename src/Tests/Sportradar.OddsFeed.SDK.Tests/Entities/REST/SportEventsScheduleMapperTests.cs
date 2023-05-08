﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST;

public class SportEventsScheduleMapperTests
{
    private readonly IDeserializer<scheduleEndpoint> _deserializer = new Deserializer<scheduleEndpoint>();

    [Fact]
    public void MappedScheduleIsNotNull()
    {
        using var stream = FileHelper.GetResource("events.xml");
        var record = _deserializer.Deserialize(stream);
        var schedule = new SportEventsScheduleMapperFactory().CreateMapper(record).Map();
        Assert.NotNull(schedule);
        Assert.Equal(914, schedule.Items.Count());
    }
}
