/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Common.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.Entities;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    /// <summary>
    /// Basic cache export/import example
    /// </summary>
    public class CacheExportImport
    {
        private readonly ILog _log;

        public CacheExportImport(ILog log)
        {
            _log = log;
        }

        public void Run(MessageInterest messageInterest)
        {
            _log.Info("Running the OddsFeed SDK Export/import example");

            _log.Info("Retrieving configuration from application configuration file");
            var configuration = Feed.GetConfigurationBuilder().SetAccessTokenFromConfigFile().SelectIntegration().LoadFromConfigFile().Build();
            //you can also create the IOddsFeedConfiguration instance by providing required values
            //var configuration = Feed.CreateConfiguration("myAccessToken", new[] {"en"});

            _log.Info("Creating Feed instance");
            var oddsFeed = new Feed(configuration);

            var sportDataProvider = oddsFeed.SportDataProvider as ISportDataProviderV5;

            if (File.Exists("cache.json"))
            {
                _log.Info("Importing cache items");
                var items = JsonConvert.DeserializeObject(File.ReadAllText("cache.json"), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                sportDataProvider.CacheImportAsync(items as IEnumerable<ExportableCI>).Wait();
            }

            var sports = sportDataProvider.GetSportsAsync().Result;
            _log.Info("Exporting cache items");
            var cacheItems = sportDataProvider.CacheExportAsync(CacheType.All).Result.ToList();
            File.WriteAllText("cache.json", JsonConvert.SerializeObject(cacheItems, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
        }
    }
}
