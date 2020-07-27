var result = {};

var total = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", env.dimensions, true));

var failedDimensions = env.dimensions;
failedDimensions["IsActivityFailed"] = "True";
var failed = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", failedDimensions, true));

result.healthStatus = 0;
if (total > 50)
{
    if (failed > total * 0.1)
    {
        result.evaluatedValue = failed;
        result.severity = 4;
        result.healthStatus = 1;
        result.message = "More than 10% of queries failed."
    }
    else if (failed > total * 0.05)
    {
        result.evaluatedValue = failed;
        result.severity = 4;
        result.healthStatus = 1;
        result.message = "More than 5% of queries failed."
    }
}

// Always return stringify(result) so engine can parse results properly
return stringify(result);