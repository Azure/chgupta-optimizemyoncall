var result = {};

var notificationCount = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", env.dimensions, true));

var globalThreshold = 30 * 7000;
var regionSizes = {
    'eus': 0.5,
    'weu': 0.3,
    'sea': 0.2
};
var regionSize = regionSizes[env.dimensions.Environment.toLowerCase()];
var regionThreshold = globalThreshold * regionSize;


var variance = Math.pow(regionSize,0.4);

result.healthStatus = 0;
result.evaluatedValue = notificationCount;
result.metadata = { 'regionThreshold': regionThreshold };
if (notificationCount < regionThreshold * 0.5 * variance)
{
    result.severity = 3;
    result.healthStatus = 1;
    result.message = "PartialSync service is dequeueing much fewer notifications than expected in region: " + env.dimensions.Environment + " . Threshold: " + regionThreshold + " (region size " + regionSize + " * global threshold " + globalThreshold + ")";
}
if (notificationCount > regionThreshold * 2 / variance)
{
    result.severity = 3;
    result.healthStatus = 1;
    result.message = "PartialSync service is dequeueing much more notifications than expected in region: " + env.dimensions.Environment + " . Threshold: " + regionThreshold + " (region size " + regionSize + " * global threshold " + globalThreshold + ")";
}

// Always return stringify(result) so engine can parse results properly
return stringify(result);