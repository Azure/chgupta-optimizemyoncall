var result = {};

var total = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", env.dimensions, true));

var failedDimensions = env.dimensions;
failedDimensions["IsActivityFailed"] = "True";
var failed = Sum(fetchMetrics(env.monitoringAccount, env.metricNamespace, env.metricName, "Count", failedDimensions, true));

result.healthStatus = 0;
if (total > 50)
{
    if (failed > total * 0.25) // increased to 0.25 from 0.1
    {
        result.evaluatedValue = failed;
        result.severity = 2; // increased temporarily to 2 from 4
        result.healthStatus = 1;
        result.message = "More than 25% of queries failed."
    }
    else if (failed > total * 0.05) // increased to 0.1 from 0.05
    {
        result.evaluatedValue = failed;
        result.severity = 3; // increased temporarily to 3 from 4
        result.healthStatus = 1;
        result.message = "More than 10% of queries failed."
    }
}

// Always return stringify(result) so engine can parse results properly
return stringify(result);