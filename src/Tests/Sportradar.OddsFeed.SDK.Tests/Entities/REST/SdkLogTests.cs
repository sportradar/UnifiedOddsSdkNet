/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.IO;
using log4net;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Tests.Common;
using LoggingCommon = Common.Logging;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST;

public class SdkLogTests
{
    public SdkLogTests()
    {
        SdkLoggerFactory.Configure(new FileInfo("log4net.sdk.config"), TestData.SdkTestLogRepositoryName);
    }

    private static void PrintLogManagerStatus()
    {
        Console.WriteLine($"Number of loggers: {LogManager.GetCurrentLoggers().Length}");
        foreach (var l in LogManager.GetCurrentLoggers())
        {
            Console.WriteLine($"\tLogger: {l.Logger.Name}");
            foreach (var a in l.Logger.Repository.GetAppenders())
            {
                Console.WriteLine($"\t\t Appender: {a.Name}");
            }
        }
        Console.WriteLine($"Number of repositories: {LogManager.GetAllRepositories().Length}");
        foreach (var l in LogManager.GetAllRepositories())
        {
            Console.WriteLine($"\tRepository: {l.Name}");
        }

        Console.WriteLine(Environment.NewLine);

        var logDefault = SdkLoggerFactory.GetLogger(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
        var logCache = SdkLoggerFactory.GetLoggerForCache(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
        var logClientIteration = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
        var logFeedTraffic = SdkLoggerFactory.GetLoggerForFeedTraffic(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
        var logRestTraffic = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
        var logStatsTraffic = SdkLoggerFactory.GetLoggerForStats(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);

        LogPrint(logDefault);
        LogPrint(logCache);
        LogPrint(logClientIteration);
        LogPrint(logFeedTraffic);
        LogPrint(logRestTraffic);
        LogPrint(logStatsTraffic);

        //for (int i = 0; i < 10000; i++)
        //{
        //    LogPrint(logRestTraffic);
        //}
    }

    private static void LogPrint(LoggingCommon.ILog log)
    {
        log.Info("info message");
        log.Error(new InvalidDataException("just testing"));
        log.Debug("debug message");
        log.Warn("warn message");
        log.Error("error message");
        log.Fatal("fatal message");
    }
}
