var result = {};

var dimensions = {};
dimensions["NotificationRetryPriorityQueueLevelDimension"] = "ARM";


dimensions["TimeBucket"] = "Under2Sec";

dimensions["Environment"]="SEA";
var Under2SecSEA = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="WEU";
var Under2SecWEU = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="EUS";
var Under2SecEUS = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));


dimensions["TimeBucket"] = "Over2Sec";

dimensions["Environment"]="SEA";
var Over2SecSEA = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="WEU";
var Over2SecWEU = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="EUS";
var Over2SecEUS = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));


dimensions["TimeBucket"] = "Over10Sec";

dimensions["Environment"]="SEA";
var Over10SecSEA = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="WEU";
var Over10SecWEU = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="EUS";
var Over10SecEUS = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));


dimensions["TimeBucket"] = "Over30Sec";

dimensions["Environment"]="SEA";
var Over30SecSEA = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="WEU";
var Over30SecWEU = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="EUS";
var Over30SecEUS = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));


dimensions["TimeBucket"] = "Over1Min";

dimensions["Environment"]="SEA";
var Over1MinSEA = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="WEU";
var Over1MinWEU = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="EUS";
var Over1MinEUS = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));



dimensions["TimeBucket"] = "Over10Min";

dimensions["Environment"]="SEA";
var Over10MinSEA = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="WEU";
var Over10MinWEU = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

dimensions["Environment"]="EUS";
var Over10MinEUS = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", dimensions, true));

var inSla = Under2SecSEA + Under2SecWEU + Under2SecEUS + Over2SecSEA + Over2SecWEU + Over2SecEUS + Over10SecSEA + Over10SecWEU + Over10SecEUS;
var overSla =  Over30SecSEA + Over30SecWEU + Over30SecEUS + Over1MinSEA + Over1MinWEU + Over1MinEUS + Over10MinSEA + Over10MinWEU + Over10MinEUS;
var allSla = inSla + overSla;

var percentage = 100 * overSla / allSla;

result.healthStatus = 0;
result.evaluatedValue = percentage;
// Only emit this incident once
if (env.dimensions.Environment.toLowerCase() == "eus" && env.dimensions["TimeBucket"] == "Under2Sec")
{
    if (percentage > 3) {
        result.severity = 3;
        result.healthStatus = 1;
        result.message = "Global successful notification time is unusually high.";
    }
    else if (percentage > 5){
        result.severity = 2;
        result.healthStatus = 1;
        result.message = "Global successful notification time is unusually high.";
    }
}

// Always return stringify(result) so engine can parse results properly
return stringify(result);