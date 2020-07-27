var result = {};
var dimensions = {};

dimensions["ActivityName"] = env.dimensions.ActivityName;
var totalCount = fetchMetrics("AzureComputeDcmx", "Dcmx", "DcmxActivityCompleted", "Count", dimensions);
var tC = sum(totalCount);

dimensions["ResultType"] = "unknownfailure";
var countOfUnknownFailures = fetchMetrics("AzureComputeDcmx", "Dcmx", "DcmxActivityCompleted", "Count", dimensions);
var uF = sum(countOfUnknownFailures);

var percOfUnknown = 0.0;
if(tC > 0)
{
    var percOfUnknown = uF / tC;
}

result.evaluatedValue = percOfUnknown;
result.thresholdViolated = result.evaluatedValue >= 0.80;
result.message = "More than 80% of unknown failures in this activity in the last 4 hours!";
result.metadata = {};
result.metadata["CountOfUnknownFailures"] = uF;
result.metadata["TotalCount"] = tC;
result.metadata["ActivityName"] = env.dimensions.ActivityName;
result.severity = 4;

return stringify(result);