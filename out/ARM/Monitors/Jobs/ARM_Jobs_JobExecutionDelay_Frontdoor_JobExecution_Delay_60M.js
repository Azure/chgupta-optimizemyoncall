var result = {};
var sum99thPercentile = 0;
var violatedCount = 0;
for(var i=0; i<timeSeries.Count.length; i++)
{
   var value = timeSeries['ComputedSamplingType\99th percentile'][i];
   sum99thPercentile = sum99thPercentile + value;
}
violatedCount = sum99thPercentile/timeSeries.Count.length;
result.evaluatedValue = violatedCount;
result.severity = 4;
result.thresholdViolated = violatedCount >= 30;
return stringify(result);
