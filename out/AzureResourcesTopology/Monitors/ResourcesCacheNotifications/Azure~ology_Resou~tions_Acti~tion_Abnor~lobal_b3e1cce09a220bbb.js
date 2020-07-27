var result = {};

var dimensions = {};
dimensions["ActivityName"] = "QueueArgNotificationMessageJob.QueueNotificationAsync";
dimensions["IsActivityFailed"] = "False";

dimensions["Environment"]="SEA";
var notificationCountSea = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));
dimensions["Environment"]="WEU";
var notificationCountWeu = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));
dimensions["Environment"]="EUS";
var notificationCountEus = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));
var notificationCount = notificationCountEus + notificationCountWeu + notificationCountSea;

var targetMinuteRate = 120000;
var targetWindowRate = 30 * targetMinuteRate

result.healthStatus = 0;
result.evaluatedValue = notificationCount;
if ((notificationCount > targetWindowRate * 2 || notificationCount < targetWindowRate * 0.4) && env.dimensions.Environment.toLowerCase() === "eus")
{
    result.severity = 3;
    result.healthStatus = 1;
    result.message = "Notification service is enqueueing an unusual number of notifications than expected overall.";
}

// Always return stringify(result) so engine can parse results properly
return stringify(result);