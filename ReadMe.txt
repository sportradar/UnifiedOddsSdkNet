UnifiedOdds Feed SDK .NET library

Notice: before starting DemoProject make sure to enter your bookmaker access token in app.config file and restore nuget packages by right-clicking the solution item and selecting "Restore NuGet Packages".

The SDK is also available via NuGet package manager. Use the following command in the Package Manager Console to install it along with it's dependencies:
    Install-Package Sportradar.OddsFeed.SDK
    
The SDK uses the following 3rd party libraries which must be added via the NuGet package manager
    - id="Common.Logging" version="3.4.1"
    - id="Castle.Core" version="3.3.2"
    - id="CommonServiceLocator" version="1.3.0"
    - id="Humanizer" version="2.8.26"
    - id="log4net" version="2.0.8"
    - id="RabbitMQ.Client" version="3.6.2"
    - id="Unity" version="4.0.1"
    - id="Metrics.NET" version="0.5.5"
    - id="Microsoft.CSharp" version="4.5.0"

The package contains:
    - DemoProject: A Visual Studio 2015 solution containing a demo project showing the basic usage of the SDK
    - libs: DLL file composing the Odds Feed SDK
    - Odds Feed SDK Documentation.chm: A documentation file describing exposed entities
    - Resources containing the log4net configuration needed by the Odds Feed SDK

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

CHANGE LOG:
2022-09-20  1.55.0
CustomBet - added support for calculate-filter endpoint
Fixed saving match timeline

2022-07-15  1.54.0
Improved API data distribution to SportEventCache
Setup that each API endpoint on critical path has its own HttpClient
Fix: recovery request url when configuring custom environment

2022-06-02  1.53.0
Improvements for saving api data within caches (single item and for list of items)
Changed the timestamp for periodic deletion of old event from cache (to now-12h)
Improved logging format in LogHttpDataFetcher and SportDataProvider
Improved logging for exceptions and warnings
Added Age to FeedMessage string
Added isOpen to OddsFeed (extended with IOddsFeedV6)

2022-04-26  1.52.0
Separate HttpClient for critical (summary, player profile, competitor profile and variant market description) and other requests
Added configuration option for fast HttpClient in OperationManager (default timeout 5s)
Added GetTimelineEventsAsync to ISportDataProvider - extended with ISportDataProviderV11 
Improved merging of competitor profile
Modified sliding expiration of profile cache items to avoid GC congestion
Improved how SportDataProvider is handling exceptions
Improved metrics and logging for raw data events
Improved metrics with app and system metrics
Added metrics for SemaphorePool
Fixed exception handling in DataRouterManager
Extended RawApiDataEventArgs with RequestParams, RequestTime and Language
Other minor improvements for observability

2022-02-23  1.51.0
Added BetradarName to IRound (extended with IRoundV3)
Fix: ICompetition competitors did not expose IsVirtual correctly

2021-12-10  1.50.0
Added support for results in sportEventStatus received from api
Added new log messages during recovery requests
Improved how merging is done within Competitor
Improved connection recovery after long disconnect
Removed unnecessary locks in SportEventStatusCache
Fix: connecting to replay server with production token
Fix: some fields in raw feed message was not filled
Fix: throws exception if match, stage or draw not found exception happens
Fix: NullReference could happen getting MarketDefinition.Groups 

2021-11-18  1.49.0
Improvements for connection resilience
Added event RecoveryInitiated to IOddsFeed
Added RabitConnectionTimeout and RabbitHeartbeat to OperationManager
Improved logging regarding connection and recovery process
Changed default UF environment from Integration to GlobalIntegration
Extended StageType with Run enum value
Fix: how connection is made
Fix: case when 2 rabbit connections are made
Fix: getting the names of category for simple_tournaments

2021-10-06  1.48.0
Extended configuration with ufEnvironment attribute
Extended ITokenSetter and IEnvironmentSelector 
New values added to SdkEnvironment enum (GlobalIntegration, GlobalProduction, ProxySingapore, ProxyTokyo)

2021-09-09  1.47.0
Improved exporting/importing and merging of player profile data
Improved SemaphorePool handling
Improvement: when fetching non-cached fixture change endpoint fails due to server error, try also normal fixture endpoint
Improved tracking of last message timestamp per producer
Fix: DI for SportEventStatusCache
Fix: wrong max recovery time was used (now default is 1h)
Fix: issue with concurrency when getting missing languages for competitor profile
Fix: merging market mapping when no outcome mappings exists
Fix: how after parameter is checked when adjustAfterAge in config is set
Other minor improvements and bug fixes

2021-07-23  1.46.0.0
IStage extended with IStageV3 providing IStageStatus and method for getting match status
Added Prologue value to StageType enum
Added improvement for connection recovery when disconnection happens
FIx: implemented safe release of all internal SemaphoreSlims
Fix: added handling of variant market description when different market id between requesting and received id

2021-07-07  1.45.1.0
Fix: problem within SemaphorePool not acquiring new semaphore - waiting indefinitely
Fix: setting configuration via CustomConfigurationBuilder
Fix: exception with modified competitor players list
Fix: exporting/importing cache items

2021-06-23  1.45.0.0
Added OperationManager to provide option to set sdk values
Added option to ignore sport event status from timeline endpoint for BetPal events
Fix: exception thrown when no market mappings found

2021-06-15  1.44.0.0
Added pitcher, batter, pitch_count, pitches_seen, total_hits, total_pitches to SportEventStatus.Properties
PeriodScore - match status code 120 mapped to penalties
Improved importing,exporting competitors
Improved getting competitor players
Fix: throwing exception when no market description received from API for variant markets

2021-05-28  1.43.0.0
Extended IMatch with GetEventTimeline for single culture (extended with IMatchV3)
Property AdditionalProbabilities moved from IOutcomeOddsV2 (deleted) to IOutcomeProbabilitiesV1 (breaking change)
Now both OutcomeOdds and OutcomeProbabilities has AdditionalProbabilitites
Added MarketMetaData to IMarketWithProbabilities (extended with IMarketWithProbabilitiesV2)
Extended ITournament with GetScheduleAsync (extended with ITournamentV2)
Fix: Issue retrieving child stages from parent using  GetStagesAsync() method
Fix: corrected which market description is returned for variant markets
Improvement: optimized fetching of player/competitor profiles

2021-04-29  1.42.0.0
Added ISportDataProvider.GetPeriodStatusesAsync to fetch period summary for stages (extended with ISportDataProviderV10)
Added ICompetitionStatus with PeriodOfLadder (extended with ICompetitionStatusV1)
Added support for SportEventStatus PeriodOfLeader - added to Properties
Improved handling of outright market mappings (before for some markets there were no mappings returned)
Improved merging of Competitor data
Fix: IMarketWithOdds.GetValidMappings() returns incorrect results

2021-04-07  1.41.1.0
Fix: parsing TeamStatistics for SportEventStatus

2021-03-31  1.41.0.0
Added IEventChangeManager to IOddsFeed for periodical fixture and result change updates (extended with IOddsFeedV3)
Extending ITimelineEvent with ITimeliveEventV2 -  Changed type of property ITimelineEvent.Player from IPlayer to IEventPlayer
Added IEventPlayer.Bench property in ITimelineEventV2.Player property
Added IGoalScorer.Method in ITimelineEvent.GoalScorer property (extended with IGoalScorerV1)
Extended ICompetitor with ICompetitorV6. Added property ShortName
Extended IProductInfo with IProductInfoV1. Added property IsInLiveMatchTracker
Improved how internal cache sport event items handle competitor lists
Changed ExportableCurrentSeasonInfoCI, ExportableGroupCI, ExportableTournamentInfoCI to return Competitors ids as list of string instead of ExportableCompetitorCI
Improved caching of competitors data on tournaments, seasons
Reverted populating Round Name, GroupName and PhaseOrGroupLongName to exactly what is received from API
Updated FixtureChangeType - also if not specified in fixtureChange message returns FixtureChangeType.NA
Added period_of_leader to the SportEventStatus.Properties
Added StartTime, EndTime and AamsId to the IMarketMetaData
Improved connection handling when disconnected
Improved merging tournament groups (when no group name or id)
Added some logs for errors when using unaccepted token
Fix: WNS event ids can have negative value
Fix: merging tournament groups
Fix: TeamStatistics returned 0 when actually null
Fix: EventResult Home and Away Score could be returned as 0, when actually null
Fix: exporting/importing season data
Fix: resolution of dependencies - removed some Guard check

2021-02-09  1.40.0.0
Added ISportDataProvider.GetLotteriesAsync (extended with ISportDataProviderV9)
Improved translation of market names (upgraded referenced library Humanizer to 2.8.26 and Dawn.Guard to 1.12.0)
Added support for eSoccer - returns SoccerEvent instead of Match
Added support for simple_team urn
Adding removal of obsolete tournament groups
Improved internal sdk processing. API calls for markets done only per user request. Optimized feed message validation.
Fix: for a case when sdk does not auto recover after disconnect

2020-12-15  1.39.0.0
Extended ILottery with GetDraws (ILotteryV1) to return list of IDraw (not just ids)
Extended ISportDataProvider with GetSportEvent (ISportDataProviderV8) so also IDraw can be obtained
Fix: getting fixture from API when result is tournamentInfo
Fix: added removal of obsolete EventTimeline events
Fix: not getting tournament data for stages

2020-12-04  1.38.1.0
Fix: getting ScheduleForDay endpoint when no events throw exception
Fix: missing totalStatistics in SoccerStatus.Statistics
Fix: soccer events not instance of ISoccerEvent
Fix: getting null fetching sport and parent stage info for stages

2020-11-13  1.38.0.0
Added new stage types in StageType enum (Practice, Qualifying, QualifyingPart, Lap)
Added CashoutStatus to IMarketWithProbabilitiesV1
Fix: loading sportEventType and stageType from parent stage if available
Fix: IMarketWithOdds.GetMappedMarketIDsAsync() returns multiple mappings
Fix: exception casting TournamentInfoCI to StageCI

2020-10-13  1.37.0.0
IRound - GroupName renamed to Group, added GroupName property (breaking change)
IStage extended with IStageV2 - added GetAdditionalParentStages, GetStageType (breaking change - result changed from SportEventType to StageType)
IEventResult extended with Distance and CompetitorResults (extended with IEventResultV2)
ICompetition extended with ICompetitionV2 - GetLiveOdds and GetSportEventType property
Added Course to the IVenue (extended with IVenueV2
Added Coverage to IMatch (extended with IMatchV2)
Improvements in recovery manager
Added support for markets with outcome_type=competitors
Make replay manager available before the feed is open
Improved connection error handling and reporting
Fix: exception thrown when there are no fixture changes
Fix: soccer events not instance of ISoccerEvent
Fix: entities null even though data is present on the API
Fix: event status enumeration

2020-08-19  1.36.0.0
Extended ISeasonInfo with ISeasonInfoV1 (added StartDate, EndDate, Year and TournamentId)
Fix: Problem with casting event to IStageV1
FIx: special case when recovery status does not reflect actual state - results in wrong triggering ProducerUp-Down event
Fix: URN.TryParse could throw unhandled exception
Fix: several issues with CustomBet(Manager) fixed
Fix: Export-Import breaks on missing data
Fix: Lottery throws exception when no schedule is obtained from api
Fix: missing nodeId in snapshot_complete routing key
Fix: SportDataProvider.GetActiveTournaments returned null
Fix: SportEventCache: improved locking mechanism on period fetching of schedule for a date
Fix: reloading market description in case variant descriptions are not available
Improved logging of initial message processing

2020-07-09  1.35.0.0
Added GetSportAsync() and GetCategoryAsync() to ICompetitor interface (extended with ICompetitorV5)
Throttling recovery requests
Fix: support Replay routing keys without node id
Fix: calling Replay fixture endpoint with node id

2020-06-24  1.34.0.0
Added support for configuring HTTP timeout
Added overloaded methods for fixture and result changes with filters
Updated supported languages
Removed logging of feed message for disabled producers
RawMessage on UnparsableMessageEventArgs is no longer obsolete
Changed retention policy for variant market cache 
Improved reporting of invalid message interest combinations
Fix: Synchronized Producer Up/Down event dispatching
Fix: Disposing of Feed instance
Fix: Permanent failure to open connection to feed

2020-05-11  1.33.0.0
Added FullName, Nickname and CountryCode to IPlayerProfile (extended with IPlayerProfileV2)
Added support for result changes endpoint
IMatchStatus provide nullable Home and Away score (extended with IMatchStatusV3)
Fix: MaxRecoveryTime is properly used to check for timeouts

2020-04-16  1.32.0.0
Added GetScheduleAsync to the BasicTournament (extended with IBasicTournamentV2)
Added bookmakerId to the ClientProperties
Fix: fixture endpoint on Replay
Fix: refreshing categories after complete sport data cache reload

2020-03-25  1.31.1.0
Changed Replay API URL

2020-03-23  1.31.0.0
Fix: invalid timestamp for cashout probabilities
Fix: handle settlement markets without outcomes
Fix: EventRecoveryCompleted is properly raised when snapshot completes
Fix: removed Guard.NotEmpty() on collections

2020-03-16  1.30.0.0
Fix: added IOutcomeSettlementV1.OutcomeResult instead of IOutcomeSettlement.Result (obsolete)
Fix: competitor references for seasons
Fix: failing API requests on some configurations

2020-02-18  1.29.0.0
Added State to the Competitor (extended with ICompetitorV4)
Added State to the Venue (extended with IVenueV1)
Extended ISportInfoProvider.DeleteSportEventFromCache with option to delete sport event status
Improved logging for connection errors
Improved fetching fixtures for Replay environment
Fix: calling variant endpoint only if user requests market data
Fix: NullPointerException in ReplayManager

2019-12-09  1.28.0.0
Added new Replay API endpoints (FEEDSDK-1316)
Internally replaced Code.Contracts with Dawn.Guard library
Fix: fetching outcome mappings for special markets that exists only on dynamic variant endpoint (FEEDSDK-1314)
Fix: outcome mapping uses product market id if available (FEEDSDK-1312)
Fix: returning market decimal value in culture invariant format

2019-12-09  1.27.0.0
Added GetReplayEventsInQueue to the ReplayManager (extended with IReplayManagerV2)
Added example for parsing messages in separate thread
Improved fetching and loading PeriodStatistics
Improved logic for getting player profiles
Improved logic for parsing round groupId
Fix: handling null markets in MarketMessage

2019-11-08  1.26.0.0
Fix: better market description cache handling
Fix: Season.GetCompetitorsAsync returns competitors from groups if needed

2019-10-24  1.25.0.0
Added cache state export/import
Added property AgeGroup to the Competitor (extended with ICompetitorV3)
Added property GreenCards to the TeamStatistics (extended with ITeamStatisticsV2)
Added IAdditionalProbabilities to the OutcomesOdds (extended with IOutcomesOddsV2)
Fix: replay ignores messages from inactive producers
Fix: green card can be null in sport event statistics

2019-09-05  1.24.0.0
Exposed option to delete old matches from cache - introduced ISportDataProviderV4
Loading home and away penalty score from penalty PeriodScore if present
Fix: return types in ISportDataProviderV3 (breaking change)
Fix: updated CustomConfigurationBuilder not to override pre-configured values
Fix: OutcomeDefinition null for variant markets
Fix: ProfileCache - CommunicationException is not wrapped in CacheItemNotFoundException
Fix: schedule date between normal and virtual feed synchronized
Fix: SportDataProvider methods invokes API requests for correct language

2019-07-19  1.23.1.0
Fix: ReplayFeed init exception

2019-07-18  1.24.0.0
Added Gender property to the IPlayerProfileV1
Added DecidedByFed property to the IMatchStatusV2
Added RaceDriverProfile property to the ICompetitorV2
Added GetExhibitionGamesAsync() to the IBasicTournamentV1 and ITournamentV1
Added Id property to the IGroupV1
Added TeamId and Name properties to the ITeamStatisticsV1
Added support for List sport events - ISportDataProvider extended with ISportDataProviderV2
Added support for TLS 1.2
Added GetAvailableTournamentsAsync(sportId) and GetActiveTournamentsAsync() to the ISportDataProviderV3
Fix: when sdk connects and API is down, UF SDK waits for next alive to make recovery
Fix: not loading variant market data in multi-language scenario
Fix: removed making whoami request in Feed ctor
Fix: on Feed.Open exception, the Open state is reset
Fix: NPE for validating market mappings when there are none

2019-06-21  1.22.0.0
Added GetStartTimeTbdAsync and GetReplacedByAsync to the ISportEventV1
Added properties StripesColor, SplitColor, ShirtType and SleeveDetail to the IJerseyV1
Improved on updating when new outcomes are available (outrights)
Exposed option for user to receive raw feed and api data
PeriodScore.MatchStatusCode no more obsolete
Fix: unnecessary api calls for competitor profiles

2019-06-07  1.21.0.0
Added property Gender to the ICompetitorV1
Added property Division to the ITeamCompetitorV1
Added property StreamUrl to the ITvChannelV1
Added property Phase to the IRoundV2
ICompetitionStatus.Status no more obsolete (fixed underlining issue)
Improved caching of variant market descriptions
Fix: caching the category without tournament failed
Fix: event status and score update issue
Fix: IMarketDescription interface exposed to user
Fix: error fetching data for sport event throws exception when enabled exception handling strategy
Fix: ReplayManager - the parameter start_time fixed

2019-05-23  1.20.1.0
Fix: unable to initialize feed

2019-05-22  1.20.0.0
Added support for custom bets
Added CustomBetManager, MarketDescriptionManager and EventRecoveryCompleted event to the IOddsFeedV2 (extends IOddsFeed)
Added GetCompetition method without sportId parameter to the ISportDataProviderV1 (extends ISportDataProvider)
Added GetFixtureChanges to the ISportsInfoManagerV1 interface (extends ISportsInfoManager)
Exposed OutcomeDefinition on IOutcomeV1 (extends IOutcome)
Exposed option to reload market descriptions
Fix: creating session with MessageInterest.SpecificEventOnly
Fix: exception when getting data for tournament with no schedule
Fix: calling TournamentInfoCI.GetScheduleAsync() from multiple threads
Fix: IMarketMappingData, IOutcomeMappingData moved from internal to public namespace

2019-04-18  1.19.0.0
Added property GroupId to the Round interface - IRound extended with IRoundV1
Improved handling of SportEventStatus updates
Improved name fetching for competitors
Fix: fixed legacy market mappings
Fix: incorrect message validation

2019-04-08  1.18.0.0
Added GetDisplayIdAsync to the IDrawV1
Added support for non-cached fixture endpoint
Improved fetching logic for the summary endpoint
Made IAlive interface internal
Fix: handling pre:outcometext and simpleteam ids in cache
Fix: IMarket.GetNameAsync - removed concurrency issue
Fix: added missing ConfigureAwait(false) to async functions

2019-03-12  1.17.0.0
Added property Grid to the EventResult interface - IEventResult extended with IEventResultV1
Added property AamsId to the Reference - IReference extended with IReferenceV1
ICompetitionStatus.Status deprecated
Instead added GetEventStatusAsync to ICompetition - extended with ICompetitionV1
IMatch extended with IMatchV1, IStage extended with IStageV1
Added Pitchers to the ISportEventConditionsV1 interface
Added enum option EventStatus.MatchAboutToStart
Added support for simpleteam competitors and related API calls
Take recovery max window length for each producer from api (all producers)
Added runParallel argument on StartReplay method on ReplayManager
Improved speed of data distribution among caches (data received from api)
Fix: IMarket names dictionary changed to ConcurrentDictionary
Fix: how PeriodScore data is saved and exposed
Fix: if the venue data is obtained from date schedule is cached so no summary request is needed

2019-02-14  1.16.0.0
Exposed Timestamps on IMessage
Added RecoveryInfo (info about last recovery) to the Producer - extended with IProducerV1
Added support for replay feed to the Feed instance
Cache distribution goes only to caches that process specific dto type
Added SDK examples based on replay server
Fix: Sport.Categories now returns all categories, delayed fetching until needed
Fix: fixed legacy market mappings (mapping to Lo or Lcco market mapping)
Fix: calling GetSportAsync from multiple threads
Fix: Competitor returning null values for AssociatedPlayers

2019-01-07  1.15.0.0
ICoveredInfo extended with ICoveredInfoV1 - exposed CoveredFrom property
Added GetOutcomeType method to IMarketDefinition (to replace GetIncludesOutcomesOfType)
Competitor qualifier is loaded with summary (before fixture)
Fix: ICompetitor.References - fixture is fetched only if competitor references are explicitly requested by user
Fix: avoiding fetching fixture for BookingStatus when received via schedule
Fix: improved locking mechanism in SportEventStatusCache to avoid possible deadlock
Fix: added check before fetching summary for competitors when already prefetched
Other minor fixes and improvements

2018-12-18  1.14.0.0
IOddsChange extended with IOddsChangeV1 - added OddsGenerationProperties
Replay session using any token returns production replay summary endpoint
Added support for custom api hosts (recovery for producers uses custom urls)
Improved locking mechanism when merging data in cache items
Added support for custom api hosts (recovery for producers uses custom urls)
Added Season start time and end time to exposed dates
Renamed Staging to Integration environment
Updated examples
Fix: filling referenceIds
Fix: SportEventCache fix for requester merge (TournamentInfoCI cast problem)
Fix: updated BetSettlementCertainty enum values to better reflect values received in feed message

2018-11-16  1.13.0.0
Extended IOddFeed with IOddsFeedV1 - new property BookmakerDetails available on OddsFeed
Improved handling of competitor reference id(s)
Removed purging of sportEventStatus from cache on betSettlement message
Fix: minimized rest api calls - removed calls for eng, when not needed (requires language to be set in configuration)
Fix: when sdk returned field null value although it is present; also avoids repeated api request

2018-10-17  1.12.0.0
Extended ITimelineEvent with ITimelineEventV1 - added MatchClock and MatchStatusCode properties
Extended IMatchStatus with IMatchStatusV1 - added HomePenaltyScore and AwayPenaltyScore (used in Ice Hockey)
Added more logs in ProducerRecoveryManager when status changes
Added more explanation in log message when wrong timestamp is set in AddTimestampBeforeDisconnect method
Added warn message for negative nodeId (negative id is reserved for internal use only)
Removed logging of response headers in LogProxy class on debug level
Fix: calling the summary with nodeId if specified (on replay server) - sport event status changes during replay of the match
Fix: Improvement of fetching and caching of SportEventStatus when called directly
Fix: schedule on CurrentSeasonInfo was not filled with data from API

2018-09-18  1.11.0.0
Added new event status: EventStatus.Interrupted
Market / outcome name construction from competitor abbreviations (if missing)
Improved logging in DataRouterManager
Improved recovery procedure (to avoid The recovery operation is already running exception)
Fix: exception during fetching event fixture or summary is exposed to user (for ExceptionHandlingStrategy.THROW setting)
Fix: error when associated players method returning null
Fix: MarketMapping for variant markets returns correctly
Fix: remove sportEvent from cache on betStop and betSettlement message (to purge its sportEventStatus)

2018-08-24  1.10.0.0
Added support for outcome odds in different formats
Introduced IOutcomeOddsV1 extending IOutcomeOdds with method GetOdds(OddsDisplayType)
ReplayServer example updated with PlayScenario
Increased RabbitMQ.Client library to 3.6.9
Fix: repaired Schedule and ScheduleEnd times for simple_tournament events
Fix: ISeason.TournamentInfo.Names contains all fetched values (before only first)

2018-07-23  1.9.0.0
Exposed property TeamFlag and HomeOrAwayTeam on IPlayerOutcomeOdds
Added support for Virtual Sports In-play message type
Added Pitcher info on SportEventConditions
Update: removing draw events from cache on DrawStatus change
Update: variant markets expiration time set to 1h
Update: added logging for sync fetcher
Fix: internally updating competitors when competitors changes on sportEvent
Fix: generation of sportEvent name with $event name template
Fix: DataRouterManager - updated logs when fetching tournament seasons
Fix: generating mapped outcomes with $score sov which dont have attribute is_flex_score
Fix: thread safety issue in ProductRecoveryManager class
Fix: statistics properties loaded from config section when needed; added CacheManager statistics
Fix: NameProvider.GetOutcomeNameFromProfileAsync returns result for ILongTermEvent without competitors

2018-06-11	1.8.0.0
Added property RotationNumber to ReferenceIds
Mapped outcomes supports translatable name
Added support for multiple market and outcome mappings
Added mapped marketId to the IOutcomeMapping
Outcome mappings for flex_score_markets take into account the score specifiers	
Removed checks for requestId on feed messages
Exposed ScheduledStartTimeChanges on IFixture
Ensured fixture change messages are dispatched only once
Improved generation of outcome names for sr:players
Improved handling of caching for simpleteam competitor and betradarId for simpleteams
In SportEventStatus in the Properties only int values are saved for Status and MatchStatus properties
Updating cached sport event booking status when user books event
Fix: Child stage links to parent stage or parent tournament
Fix: loading of EventResult
Fix: SportEventCache cannot fetch multiple times for the same date
Fix: ICompetition.GetConditionsAsync did not load for all cultures
Fix: saving-caching special tournament data
Fix: Venue on competition

2018-04-26  1.7.0.0
Added CountryCode property to IVenue
Added AdjustAfterAge property to Section and ConfigurationBuilders
Added local time check against bookmaker details response header
Added PhaseOrGroupLongName property to IRound
WNS endpoints are called only if WNS available
Improved handling of sport event status obtained from feed or API
All recovery methods takes into account the specified nodeId
Fix: missing market description or outcome no longer throws exception
Fix: loading of tournament data for season events
Fix: print of SportEventConditions
Fix: FixtureDTO mapping for ReplacedBy property
Internal: added exception handling strategy to DataRouterManager
Other minor fixes and improvements

2018-04-03	1.6.0.0
New configuration builders
Added support for replaying of events messages generated by a specific producer
Removed restrictions on speed and maxDelay in ReplayManager
Exposed RawMessage on a IEventMessage<T> (returns byte array received from feed)
Removed asynchronous call within Feed ctor
Added 'Postponed' value to enum EventStatus
Introduced new decimal properties to IEventResult: PointsDecimal, WcPoints, ClimberDecimal and SprintDecimal
Added properties ReplacedBy, NextLiveTime and StartTimeTBD to IFixture
Fix: exception when season has no groups
Fix: getting TournamentInfo on Season instance
Fix: displaying correct CurrentSeasonInfo on ISeason
Fix: loading competitors for stage events, loading category for stage events
Fix: loading draw events for lottery;
Fix: loading draw fixture
Internal: improved how recovery is made
Internal: improvements in caches to avoid exceptions, locks, ...
Other minor fixes and improvements

2018-03-01  1.5.0.0
Exposed VoidReason on BetCancel, BetSettlement, ... (introduced IMarketCancel interface)
Added property Timestamp to all feed messages (time when message was generated)
Updated IPeriodScore to reflect new rest and feed periodScoreType
Added GetMatchStatusAsync on IPeriodScore and IEventResult (it returns the same value as on IMatchStatus)
Updated some types of properties on IEventResult to correctly reflect those on Sport API
Improved handling of recovery and producer status based on feed messages
Internal: CacheManager made non-static - now supports multiple feed instances
Internal: hardening of cache handling for automatically fetching data
DemoProject updated
Fix: updated how IReplayManager.AddSportEventToReplay behaves when startTime is not specified
Fix: fixed how variant markets are cached (includes player props markets)
Fix: ITournament.GetSeasonsAsync always returned null
Fix: running multiple feed instances at the same time or scenario open-close-open of the same instance
Fix: correctly handling of decimal value in outcome name with +/- name template
Other minor fixes and improvements, improved internal exception handling

2018-02-12  1.4.0.0
Added support for WNS/lottery (new IDraw and ILottery sport event)
Added property DelayedInfo to IMatch
Added property ProductsId to IMarketMapping
Internal: optimization of data distribution amoung sdk caches
Fix: MarketCacheProvider throwing exception when no market description found
Fix: handling of logging exception when error happens receiving data from Sports API
Other minor improvements and fixes

2018-01-29  1.3.0.0
Added support for $event name template
Added support for match booking
Updated support for variant markets
Removed log4net logging library and introduced Common.Logging library. For an example of configuration using log4net & Common.Logging please check the latest SDK example.
OddsFeedConfigurationBuilder enables the user to specify the connection should be made to staging environment
Fix: Category property on ISport and ITournament always returned null
Other minor improvements and bug fixes

2018-01-15  1.2.0.0
SportEvent hierarchy overhaul - 'stage' support
Improved recovery process - on slow message processing, producer is marked as down
Added Certainty to BetSettlement message
Added support for Market MetaData on oddsChange message
Added support for multiple Groups on BetStop message
Exposed additional market info through market definitions
Added support for SoccerEvent, SoccerStatus
Expanded PlayerProfile and Competitor to support Manager, Jersey info
Exposed event timeline on IMatch entity
Exposed tournament seasons on ITournament entity
Exposed methods for clearing cache data on ISportDataProvider
NodeId supports negative numbers
Added support for nodeId on Replay server on all methods
Internally: changed how caches works and added CacheManager

2017-11-06	1.1.8.0
Improved recovery process
Support for player markets, support for empty market messages
IOddsFeed - added Closed event, invoked when feed is forcebly closed
Added property Name to ICompetion
Exposed raw xml message on all feed message received events
Fix: %server specifier template loaded from competitor profile
Fix: stateful messages not dispatched when connecting to replay server
Fix: updating cache for SportEventStatus on feed messages

2017-08-04  1.1.7.0
Fixed bug when feed recovery gets interrupted when lasting longer then expected
Empty tournament names now allowed
Added 'maxRecoveryTime' to the config section

2017-07-31  1.1.6.0
CashoutProvider enabled on feed instance
Season - fixed multilanguage name support
Messages for disabled producers are no longer dispatched
Fixed clearing SportEventStatusCache on bet_settlement message
Fixed that on disconnection / on productdown event is raised correctly when no message arrives within specified timeout

Version 1.1.6.0 contains breaking changes:
Product enumeration has been replaced with IProducer interface in order to ensure automatic support of new producers
Some functionality previously exposed on the IOddsConfiguration interface has been moved to new IProducerManager interface. 

2017-06-15	1.1.5.0
All SportEventStatus properties available in Properties list
Ensured thread safety in SportDataCache when new Tournament data is added/updated
Updated how SportEvent or Tournament Season data is obtained and available for users
Product code for LCoO changed to 'pre'
Updated market mapping validator to be culture invariant

2017-06-01  1.1.4.0
Fixed deserializing of API messages for race details
Changed how sport event status is obtained internally, made available via ISportDataProvider
Added auto-close to feed when recovery request can not be made

2017-05-29	1.1.3.0
Changed support for wild card validity market mapping validity attributes - now the format "specifer_name~*.xx" is supported
Fixed a bug in session message handling which caused some of the messages were not dispatched to the user
Modified handling of error response codes returned by the recovery endpoints 

2017-05-23 	1.1.2.0
Added support for x.0, x.25, x.5, x.75 market mapping validity attribute
Added support for (%player) name template in market name descriptions

2017-04-21 	1.1.0.0
Breaking Changes:
IMarketWithOdds.IsFavourite renamed to IMarketWithOdds.IsFavorite
IMarketWithOdds.Outcomes renamed to IMarketWithOdds.OutcomeOdds
IMarketWithSettlement.Outcomes renamed to IMarketWithSettlement.OutcomeSettlements
ISportEvent interface renamed to ICompetition
ISportEntity interface renamed to ISportEvent
IOddsChange.BetStopReason: Type changed from enum to INamedValue
IOddsChange.BettingStatus: Type changed from enum to INamedValue
Introduced IOddsFeedConfigurationBuilder which must now be used in order to create IOddsFeedConfiguration
RecoveryRequestIssuer property on the IOddsFeed renamed and it’s type changed

New Features:
Improvements & bug fixes to recovery process
Initial support for upcoming products BetPal & PremiumCricket added
Enforcing that timestamp for recovery is not more than 72 hours in the past or in the future
Added support for connection to custom message broker / RESTful api
Optimization of resources used by the connection to the message broker

2017-04-19 	1.0.3.0
Support for flexible score markets
Added support for outcomes with composite ids
Void reason added to betsettlement messages
Market status property added to betstop messages
Minor fixed how recovery are called internaly

2017-03-29 	1.0.2.0
Fixed calling groups on Tournament when no groups are present
Fixed internal mapping for Odds messages without markets

2017-03-31 1.0.1.0
Removed UnalteredEvents event and message
Added encoding UTF-8 for logging in log4net.sdk.config (check DemoProject)
Fixed internal message processing pipeline for multi-session scenario
Implemented support for simple math expressions in market names
Removed Group from SportEvent
SportDataProvider - fixed retrieving TournamentSchedule
Made some properties of the REST entities translatable due to changes in the feed 
  
2017-02-06 Official version (1.0.0)
Updated fixture changes (added References to SportEvent and Competitor object)
Added event statuses
Added MatchStatus in SportEventStatus object
Added support for Replay Server
Added support for Market and Outcomes mappings
Improved how recoveries are made (user can specify when last message was processed)
New DemoProject examples added
Performance improvements

2016-11-21 Release candidate (0.9.1)
SDK available via nuget package manager
SDK merged into a single assembly which only exposes types required by the user
Additional market statuses added

2016-11-04 Release candidate (0.9.0)
Outright support
Added support for generating market names
Ensured no exceptions are thrown to the external callers
Improved recovery
Multiple log files for easier debugging
Support for specific event handlers
Added diagnostic tools to determine the cause of potential connection issues
Support for multiple languages
Improved caching of sport related data

2016-07-04 Beta Version
Added support for retrieving information about sport events associated with each odds message
Added support for initial synchronization with the feed
Added support for detection of the out-of-sync situation and automatic re-sync
BetStop messages now also contain identifiers of the markets which needs to be stopped (not just a market group)

2016-06-17 Initial Version (alpha)