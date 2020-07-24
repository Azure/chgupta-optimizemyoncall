//------------------------------------------------------------------------------
// This is a sample project to use Geneva Health Client SDK, please check the
// following wiki links for more information:
// Prerequisites:           https://jarvisdocumentation.azurewebsites.net/alerts/Management/setup.html
// Consume Health Data:     https://jarvisdocumentation.azurewebsites.net/consume/alerting/consumehealth.html
// Manage Monitors:         https://jarvisdocumentation.azurewebsites.net/alerts/Management/managemonitors.html
// Manage Resource Types:   https://jarvisdocumentation.azurewebsites.net/alerts/Management/manageresourcetypes.html
// Manage Other Configs:    https://jarvisdocumentation.azurewebsites.net/alerts/Management/manageaccountconfigs.html
// When open the project through VS, please uninstall the old Microsoft.Cloud.HealthService.Client.2.0.14
// nuget package and install the latest version from MSBLOX. The wiki link above includes package installation
// location and instruction.
//------------------------------------------------------------------------------

namespace HealthClientSDKSample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Cloud.HealthService.Client;
    using Microsoft.Cloud.HealthService.Client.Configuration;
    using Microsoft.Online.Metrics.Common.EventConfiguration;
    using Microsoft.Online.RecoveryService.Contract.Models;

    public class Program
    {
        // The client for consuming and submitting Health data (reports, suppression, status, etc)
        static HealthServiceClient _healthClient = null;

        // The manager for managing Monitor Configurations.
        private static MonitorConfigurationManager _monitorConfigurationManager = null;

        // The manager for managing Topology Configurations (Resource Type, IcM, Service Bus)
        private static TopologyConfigurationManager _topologyConfigurationManager = null;

        public static void Main()
        {
            Trace.AutoFlush = true;

            using (
                Stream traceStream =
                    new FileStream(
                        @"c:\temp\healthtraceFromCode.log",
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.Read))
            {
                Trace.Listeners.Add(new TextWriterTraceListener(traceStream));

                Program.Run();
            }
        }

        private static void Run()
        {
            string clientCertThumbprint = "8A086626C94B976403E08CF454339CEED7154BA9";

            // Instantiate the default Health Service Client implementation from the library
            // Choose the execution environment that your account is hosted in.
            _healthClient = new HealthServiceClient(
                ExecutionEnvironment.Production,                 // The Health System environment (PROD or INT).
                clientCertThumbprint,      // The client thumbprint.
                useCurrentUserStore: false,                       // The certificate store. false = LocalMachine, true = CurrentUser
                retryCount: 3,                                   // The retry count for API calls.
                retrySleepInterval: 1,                           // The interval between retries (in seconds).
                batchSize: 20,                                   // This is used in batch submit APIs
                connectionCacheTime: TimeSpan.FromHours(6));     // This is used to periodically refresh the URLs to the instance your
                                                                 //  account is located on in the case of account migration    

            // Create the ResourceIdentifier to be used in the API calls.
            var resourceDimensions = new Dictionary<string, object>()
            {
                {"App", "FooBarService"},
                {"Region", "West US"},
                {"Datacenter", "CO2"},
                {"RoleInstance", "CO2BFK083"}
            };

            var testResourceId = new ResourceIdentifier(
                "GenevaHealthDemo",     // Your monitoring account name
                "RoleInstance",         // The resource type
                resourceDimensions);

            // Submit watchdog report on a resource
            SubmitWatchdogReportAsync(testResourceId).Wait();

            // Query for the current health status of a resoure: 
            GetCurrentHealthStatusAsync(testResourceId).Wait();

            // Query health of the resource
            QueryResourceHealthAsync(testResourceId).Wait();

            // Query watchdog reports of a resource
            QueryWatchdogReportsAsync(testResourceId).Wait();

            // Query resource health history
            QueryResourceHealthHistoryAsync(testResourceId).Wait();

            // Sets suppression on the resource
            SetAndGetSuppressionStateAsync(testResourceId).Wait();

            // Add and get annotations on the resource/watchdog
            AddAndGetAnnotationsAsync(testResourceId).Wait();

            // Instantiate the Monitor Configuration Manager:
            _monitorConfigurationManager = new MonitorConfigurationManager(
                new Uri("https://prod4.metrics.nsatc.net/"),    // The frontend endpoint of the environment your acocunt is hosted in.
                clientCertThumbprint,     // The client thumbprint.
                StoreLocation.LocalMachine,                      // Which store to find the client certificate in.
                TimeSpan.FromMinutes(2));                       // The time out for calls.

            // Read and update monitors:
            ReadAndManageMonitorsAsync().Wait();

            // Instantiate the Topology Configuration Manager:
            _topologyConfigurationManager = new TopologyConfigurationManager(
                new Uri("https://prod4.metrics.nsatc.net/"),    // The frontend endpoint of the environment your acocunt is hosted in.
                clientCertThumbprint,     // The client thumbprint.
                StoreLocation.LocalMachine,                      // Which store to find the client certificate in.
                TimeSpan.FromMinutes(2));                       // The time out for calls.

            ReadAndManageResourceTypesAsync().Wait();

            ReadAndManageServiceBusConfigurationAsync().Wait();

            ReadAndManageIcmConfigurationAsync().Wait();
        }

        private static async Task SubmitWatchdogReportAsync(ResourceIdentifier testResourceId)
        {
            // Pre-Requisites
            // 1) You have authorized the certificate thumbprint in your monitoring account. ("Access" section in "Account" tab).
            // 2) You have created resource-type configuration in the topology of your monitoring account ("resource-types" section in "Account" tab).

            // Submit the watchdog report.

            // First build individual WatchdogReport object.
            // Create report metadata which will contain properties like Severity, ReportExpirationTime,
            // Title etc. which will be used to create incidents with specified severity and Title.
            var testWatchdogMetadata = new MetadataCollection(
                new Dictionary<string, object>()
                {
                    {"Severity", 2},
                    {"ReportExpirationTime", 120},
                    {"Title", "This is the title which will be put in the incident"}
                });

            // Create resource metadata which properties not part of resource-id 
            var testResourceMetadata = new MetadataCollection(
                new Dictionary<string, object>()
                {
                    {"IP", "10.2.3.4"}
                });

            // Create report.
            var testReport = new WatchdogReport()
            {
                WatchdogName = "Test watchdog",
                WatchdogType = WatchdogType.Periodic,
                ResourceId = testResourceId,
                WatchdogMetadataCollection = testWatchdogMetadata,
                ResourceMetadataCollection = testResourceMetadata, // Can also be null
                Status = ResourceHealthStatus.Error,
                Message = "Alert fired via health monitor"
            };

            // Call SubmitBatchWatchdogHealthReports with List<WatchdogReport> collection.
            var resultList = await _healthClient.SubmitBatchWatchdogHealthReports(
                new List<WatchdogReport> { testReport });

            // Walk through the failed results.
            if (resultList.Count > 0)
            {
                // Some health report submission fails, walk through the returned list to find out the reason.
                foreach (WatchdogReportResult result in resultList)
                {
                    // Most common failures
                    // 1) Certificate used in not authorized in your monitoring account.
                    // 2) Resource-type configuration is not created in the topology of your monitoring account. (resource topology name == monitoring account)
                    // 3) Make sure that you are referring to the latest nuget package in this project. Refer to top of this file.
                }
            }
        }

        public static async Task QueryResourceHealthAsync(ResourceIdentifier testResourceId)
        {
            // Query health report of the resource

            // Create filter to only include watchdogs which have "UsedForDeploymentValidation" set to true in watchdogMetadata.
            var healthFilter = new HealthFilter()
            {
                Operation = HealthFilterOperation.Equals,
                PropertyName = "UsedForDeploymentValidation",
                PropertyValue = "true"
            };

            // Get the resource health since last 10 mins. This will return healthy only when the resource has been healthy
            // for last 10 mins o/w return error/degraded/unknown status along with reason.
            var healthReport = await _healthClient.GetResourceHealthReport(
                testResourceId,
                new List<HealthFilter>() {healthFilter},
                DateTime.UtcNow - TimeSpan.FromMinutes(10)); // "since" can be null to get the latest health report.

            // If the health status is not healthy then get the reason.
            var healthStatus = healthReport.HealthStatus;
            if (healthReport.HealthStatus != ResourceHealthStatus.Healthy)
            {
                var unhealthyReason = healthReport.HealthStatusReason;
            }
        }

        public static async Task QueryWatchdogReportsAsync(ResourceIdentifier testResourceId)
        {
            // Query all health reports of the resource, exclude expired reports (older than 24 hours).
            var activeReportList = await _healthClient.GetWatchdogReports(testResourceId);
            foreach (WatchdogReport report in activeReportList)
            {
                // operate on the returned WatchdogReport objects.
            }

            // Query health reports of the resource, include the expired reports (older than 24 hours)
            var allReportList = await _healthClient.GetAllWatchdogReports(testResourceId);
            foreach (WatchdogReport report in allReportList)
            {
                // operate on the returned WatchdogReport objects.
            }
        }

        public static async Task GetCurrentHealthStatusAsync(ResourceIdentifier testResourceId)
        {
            // Query resource health (at this current moment)
            var resourceHealth = (await _healthClient.GetHealthData(testResourceId))?.HealthStatus;
        }
        
        public static async Task QueryResourceHealthHistoryAsync(ResourceIdentifier testResourceId)
        {
            // Query resource health history during a time-range
            var healthHistory = await _healthClient.GetBatchWatchdogHealthHistory(
                testResourceId.TopologyName,
                new List<ResourceIdentifier> {testResourceId},
                new List<HealthFilter>(),
                DateTime.UtcNow - TimeSpan.FromHours(1),
                DateTime.UtcNow);
        }

        public static async Task SetAndGetSuppressionStateAsync(ResourceIdentifier testResourceId)
        {
            // sets suppression on the resource
            await _healthClient.SetSuppressionState(
                testResourceId,
                SuppressionMode.Suppress,
                TimeSpan.FromDays(1));

            Thread.Sleep(TimeSpan.FromMinutes(1));

            // checks if the resource is suppressed or not. It can take upto 1 min for suppression to take affect on 
            // the resource and its children.
            var suppressionState = await _healthClient.GetSuppressionState(testResourceId);
        }


        public static async Task AddAndGetAnnotationsAsync(ResourceIdentifier testResourceId)
        {
            // Submit annotation
            var annotationMetadata = new Dictionary<string, string>()
            {
                {"DeploymentTime", DateTime.UtcNow.ToString()},
                {"DeployedBy", "TestUser"},
                {"DeploymentLink", "http://deployment/results?id=1"}
            };

            var annotation = new HealthAnnotation(
                testResourceId,
                "Test Watchdog",    // If null, the annotation will be put just on the resource
                "There was a deployment on the resource",
                annotationMetadata,
                DateTime.UtcNow);   // The time can be anything in the past as well.

            await _healthClient.AddorUpdateHealthAnnotationBulk(new List<HealthAnnotation>() {annotation});

            Thread.Sleep(TimeSpan.FromSeconds(30));

            var annotations = await _healthClient.GetHealthAnnotationsForWatchdog(
                testResourceId,
                "Test Watchdog",
                DateTime.UtcNow - TimeSpan.FromMinutes(2),
                DateTime.UtcNow);
        }

        public static async Task ReadAndManageMonitorsAsync()
        {
            // === READ MONITORS ===
            // This gets all the monitors in the account "Geneva Health Demo".
            List<MonitorConfiguration> monitorsInAccount =
                await _monitorConfigurationManager.GetMonitors("GenevaHealthDemo");

            // This gets a filtered list of monitors in the account "Geneva Health Demo"
            // and the namespace "RequestMetrics".
            List<MonitorConfiguration> monitorsInNamespace = await _monitorConfigurationManager.GetMonitors(
                "GenevaHealthDemo",
                @namespace: "RequestMetrics");

            // This gets a filtered list of monitors in the account "Geneva Health Demo"
            // and the namespace "RequestMetrics" and the metric "HttpRequests".
            List<MonitorConfiguration> monitorsInMetric = await _monitorConfigurationManager.GetMonitors(
                "GenevaHealthDemo",
                @namespace: "RequestMetrics",
                metric: "HttpRequests");

            // This gets a filtered list of monitors in the account "Geneva Health Demo"
            // and the namespace "RequestMetrics" and the metric "HttpRequests" with the name "HighNumErrors".
            // Although this returns a list, because a monitor is unique in its account/namespace/metric/name, 
            // this will only return 1 monitor.
            List<MonitorConfiguration> specificMonitor = await _monitorConfigurationManager.GetMonitors(
                "GenevaHealthDemo",
                @namespace: "RequestMetrics",
                metric: "HttpRequests",
                monitor: "HighNumErrors");

            // === ADD NEW MONITOR ===
            MonitorConfiguration newMonitor = new MonitorConfiguration
            {
                EventIdentifier = specificMonitor[0].EventIdentifier, // Create the new monitor under the same metric as the one we queried for
                Id = "NewMonitorTesting",                       // The name of the monitor
                Frequency = TimeSpan.FromMinutes(5),            // The monitor execution frequency
                LookbackDuration = TimeSpan.FromMinutes(10),    // The monitor lookback duration
                Conditions = specificMonitor[0].Conditions,     // We will use the same data source as the HighNumErrors monitor
                ResourceType = "RoleInstance",                  // The resource type that this monitor monitors
                TemplateType = MonitorTemplateType.Sum2,        // The monitor template type
                Thresholds = new List<MonitorThreshold>         // These are the alert conditions
                {
                    new MonitorThreshold
                    {
                        Comparator = ">",
                        HealthStatus = MonitorResultHealthStatus.Error,
                        Severity = 3,
                        Value = 10
                    }
                },
                TemplateSpecificParameters = new Dictionary<string, string>
                {
                    { "metric", "Sum" },
                }
            };
            
            // Make the call to add the monitor:
            // Because the monitor does not exist, it will perform an addition.
            await _monitorConfigurationManager.AddOrUpdateMonitors(new List<MonitorConfiguration> { newMonitor });

            // === UPDATE ===
            // Perform some updates to the monitor configuration.
            // For example, we are updating the auto-mitigation settings of this monitor:
            newMonitor.ShouldMitigateIncident = true;
            newMonitor.HealthyDurationToMitigateIncident = TimeSpan.FromMinutes(20);
            newMonitor.HealthyCountToMitigateIncident = 4;

            // Make the call to perform the update:
            // Because the monitor does exist, it will perform an update.
            await _monitorConfigurationManager.AddOrUpdateMonitors(specificMonitor);

            // === REMOVE === 
            // You can also remove a list of monitors:
            await _monitorConfigurationManager.RemoveMonitors(new List<MonitorConfiguration> { newMonitor });
        }

        public static async Task ReadAndManageResourceTypesAsync()
        {
            // == READ ==
            // This gets all the resource types in the account "GenevaHealthDemo".
            List<ResourceTypeConfiguration> resourceTypesInAccount =
                await _topologyConfigurationManager.GetResourceTypes(
                    "GenevaHealthDemo");

            // This gets a filtered list of the the resource type in the account "GenevaHealthDemo"
            // and with the name "RoleInstance".
            // Although this returns a list, because resource types are unique by their account/name,
            // this will return only 1 resource type.
            List<ResourceTypeConfiguration> resourceTypesWithName =
                await _topologyConfigurationManager.GetResourceTypes(
                    "GenevaHealthDemo",
                    "RoleInstance");

            // == ADD NEW RESOURCE TYPE == 
            // We will add a new resource type configuration:
            ResourceTypeConfiguration datacenterResourceType = new ResourceTypeConfiguration
            {
                Name = "SDKTestDatacenter", // The name for your new Resource Type (should be unique within the account)
                IdentifierProperties = new List<string>
                {
                    "App",
                    "Region",
                    "Datacenter"
                }, // The identifying properties for your new Resource Type
                ViewConfiguration =
                    new HealthViewTreeConfiguration // The Health Visualization configuration (how your resource type will look in the Health page)
                    {
                        Name = "Datacenter Health SDK Test", // The name of the tree
                        NodeNames =
                            new
                                List<string>() // The levels of the tree. We recommend that this includes the identifier dimensions.
                                {
                                    "{App}",
                                    "{Region}",
                                    "{Datacenter}"
                                }
                    },
                LastUpdateTime = DateTime.UtcNow,
                LastUpdatedBy = "HealthClientSDKSampleProject"
            };

            // Make the call to add the Resource Type
            // Note: If a resource type with name "Datacenter" exists, this will perform an update and not an add.
            //       If it contains different identifier properties, it will fail with a validation error, as that is immutable.
            await _topologyConfigurationManager.AddOrUpdateResourceTypes(
                "GenevaHealthDemo",
                new List<ResourceTypeConfiguration>() { datacenterResourceType });

            // == UPDATE == 
            // Perform some updates to the configuration:
            datacenterResourceType.ViewConfiguration.Name = "New View Tree Name";
            datacenterResourceType.IncidentConfiguration = new IncidentConfiguration
            {
                RoutingId = "routing://testRoutingId"
            };

            // Make the call to save the updates:
            await _topologyConfigurationManager.AddOrUpdateResourceTypes(
                "GenevaHealthDemo",
                new List<ResourceTypeConfiguration> { datacenterResourceType });

            // === REMOVE === 
            // Remove all the resource types in the list:
            await _topologyConfigurationManager.RemoveResourceTypes(
                "GenevaHealthDemo",
                new List<ResourceTypeConfiguration>{ datacenterResourceType });
        }

        public static async Task ReadAndManageIcmConfigurationAsync()
        {
            // Get the IcM configuration:
            IcmConfiguration icmConfiguration =
                await _topologyConfigurationManager.GetIcmConfiguration("GenevaHealthDemo");

            if (icmConfiguration != null)
            {
                // Perform some updates:
                icmConfiguration.ConnectorId = Guid.NewGuid();

                // Save the updates: 
                await _topologyConfigurationManager.SaveIcmConfiguration("GenevaHealthDemo", icmConfiguration);
            }
        }

        public static async Task ReadAndManageServiceBusConfigurationAsync()
        {
            // Get the service bus configuration:
            ServiceBusConfiguration serviceBusConfiguration =
                await _topologyConfigurationManager.GetServiceBusConfiguration("GenevaHealthDemo");

            if (serviceBusConfiguration != null)
            {
                // Perform some updates:
                serviceBusConfiguration.EnrichIncidentBeforeSending = true;

                // Save the updates: 
                await _topologyConfigurationManager.SaveServiceBusConfiguration("GenevaHealthDemo", serviceBusConfiguration);
            }
        }
    }
}