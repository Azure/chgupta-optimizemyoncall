var threshold2 = 0.8;
var result = {};
var dimensions = {};

try{
    var limitSeries = fetchMetrics(
        "ARM",
        "MdmQos",
        "ClientAggregatedMetricCountLimit",
        "Sum",
        dimensions
    );
}
catch(err){
    // do nothing
}

dimensions["Reason"] = "sampling";

try{
    var errorSeries = fetchMetrics(
        "ARM",
        "MdmQos",
        "DroppedClientMetricCount",
        "Sum",
        dimensions
    );
}
catch(err){
    // do nothing
}

result.evaluatedValue = 1;
result.thresholdViolated = false;
result.severity = 4;
result.message = "ClientAggregatedMetricCountLimit does not exist.";

if(limitSeries && sum(limitSeries)){
    var averageVal = sum(timeSeries.Sum) / (timeSeries.Sum.length || 1);
    var averageLim = sum(limitSeries) / (limitSeries.length || 1);
    result.evaluatedValue = averageVal / (averageLim || 1);
    result.thresholdViolated = result.evaluatedValue > threshold2;
	
	var msg1 = "This in an automatic alert from MDM. Warning: You are currently using more than ";
	var msg2 = " of your allocated quota of events per minute. If you go above 100%, your account will " +
			"be sampled and some data will be lost. ";
	var msg3 = "Your current usage is " + averageVal + " out of " + averageLim + ". " +
			"This might mean you are sending too many metrics from client machines: too many " +
			"metrics per machine and/or too many machines are sending metrics." +
			"If this is unexpected, please make sure you are not emitting more than needed. " +
			"Otherwise, you can request an increase to your current limit by sending an email to metricandhealthreq@microsoft.com. " +
			"This monitor can be disabled or customized " +
			"at Manage -> Geneva Hot Path -> ARM -> MdmQos -> ClientAggregatedMetricCount -> " +
			"Monitors. You can see your usage and more information on automatic monitoring and limits by " +
			"clicking on the links at the bottom of the description.";

    if(result.evaluatedValue > threshold2){
		result.severity = 2;
        result.message = msg1 + "80%" + msg2 + msg3;
	}
	else{
		result.message = "Your current usage is " + averageVal + " out of " + averageLim + ".";
	}

	if(errorSeries){
		var error = sum(errorSeries) > 0;
		result.thresholdViolated = result.thresholdViolated || error;
		if(error){
			result.severity = 2;
			result.message = "This in an automatic alert. Warning: You are currently using more than your " +
				"allocated quota of events per minute. Your account is being sampled: data loss is occurring. " + msg3;
		}
	}
}

return stringify(result);