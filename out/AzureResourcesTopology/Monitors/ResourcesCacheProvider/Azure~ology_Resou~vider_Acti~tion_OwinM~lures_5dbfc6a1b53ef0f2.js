var result = {};

var total = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", env.dimensions, true));

var failedDimensions = env.dimensions;
failedDimensions["IsActivityFailed"] = "True";
var failed = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", failedDimensions, true));

result.healthStatus = 0;

var sev2Threshold = total > 100 ? 0.1 : 0.5;
var sev3Threshold = total > 100 ? 0.05: 0.25;

if (failed > total * sev2Threshold)
{
    result.evaluatedValue = failed;
    result.severity = 2;
    result.healthStatus = 1;
    result.message = "More than 10% traffic failed, or more than 50% traffic failed at low traffic volume."
}
else if (failed > total * sev3Threshold)
{
    result.evaluatedValue = failed;
    result.severity = 3;
    result.healthStatus = 1;
    result.message = "More than 5% of queries failed, or more than 25% traffic failed at low traffic volume."
}

// Always return stringify(result) so engine can parse results properly
return stringify(result);