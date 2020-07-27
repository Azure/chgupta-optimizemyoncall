var result = {};

// Count how many machines are sending heartbeats
var sumOfCount = 0;

// For each time series
for (var i=0; i<timeSeries.Count.length; i++)
    sumOfCount += timeSeries.Count[i];

result.evaluatedValue = sumOfCount / timeSeries.Count.length;
if (result.evaluatedValue < 4) {
    result.severity = 3;
    result.healthStatus = 2;
    result.message = 'Alfred is down, unable to achieve quorum with only ' + result.evaluatedValue + ' replicas sending heartbeats on average over the last 30 minutes.'
} else if (result.evaluatedValue < 6) {
    result.severity = 4;
    result.healthStatus = 1;
    result.message = 'Alfred is unhealthy, with only ' + result.evaluatedValue + ' replicas sending heartbeats on average over the last 30 minutes.'
} else {
    result.healthStatus = 0;
}

return stringify(result);