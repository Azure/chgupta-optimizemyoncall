# To run this script you need the following
# 1. Download the MDConfigUtility from Geneva https://genevamondocs.azurewebsites.net/alerts/Management/managemonitors/managemonitorsutility.html
# 2. Take your machine cert from LocalMachine and whitelist in the Geneva accounts for FF and MC.
# 3. Update the cert_thumbprint below with the whitelisted cert.

$cert_thumbprint="650C83849E473A9947B9534AF8DC4B76B9918CC1"
$exe_path="F:\github\chgupta-optimizemyoncall\src\sdk\MdmConfigUtility\"
$base_path="F:\github\chgupta-optimizemyoncall\out"

foreach($ac in ("AzureComputeManager","AzureComputeDcmx")){ #, "AzureResourcesTopology"
	# Download all the configurations
	. $exe_path\MdmConfigUtility.exe /DownloadAllConfigs /Account:$ac /FoldersOnNamespacesLevel:True /Folder:$base_path\$ac /MdmEnvironment:Production /CertThumbprint:$cert_thumbprint  /CertStore:LocalMachine /MaxFileNameProducedLength:64


	}

	$ac = "ARM"
# Process the monitors and build Powershell objects
$monitors = @()
$records = @()
$monitor_files = Get-ChildItem -s $base_path\$ac\Monitors\*.json 
ForEach($file in $monitor_files) { 
	Write-Output "Processing $file"
	$fileContent = Get-Content $file
	$monitorConfig = $fileContent | ConvertFrom-Json 
	$monitors += $monitorConfig.monitors #| Where-Object isDisabled -eq $false | Where-Object isSilent -eq $false
	$monitorConfig.monitors | ForEach-Object { $record = $_.category+ "," +  $_.eventIdentifier+ "," +  $_.frequency+ "," + $_.healthyCountToMitigateIncident+ "," + `
		$_.healthyDurationToMitigateIncident+ "," + $_.hintsLookbackDuration+ "," + $_.id+ "," + $_.inputResourceType+ "," + $_.isDisabled+ "," + $_.isRegularMonitor+ "," + $_.isSilent+ "," + $_.isThrottled+ "," + `
		$_.lastUpdatedBy+ "," + $_.lastUpdateTime+ "," + $_.lookbackDuration+ "," +  $_.metricsViewName+ "," + $_.monitorActorConfiguration+ "," + $_.monitorDataSourceType+ "," + $_.raiseIncientOnMonitorFailure+ "," + `
		$_.resourceType+ "," + $_.severityForMonitorFailure+ "," + $_.shouldMitigateIncident+ "," + $_.shouldSendToAutomation+ "," +  $_.templateType+ "," + $_.tenantName+ "," + `
		$_.thresholds+ "," + $_.version+ "," + $monitorConfig.component+ "," + $monitorConfig.id+ "," + $monitorConfig.tenant ; 
		$records += $record.replace("[","").replace("]","").replace("`n","")
	}
	# Change System Object references and [] chars
	}



$monitorProperties | ForEach-Object { 
	$type = $_.Definition.Split(" ")[0]; 
	if($type -ne "string" -and $type -ne "bool" -and $type -ne "int") {
		$type = "dynamic"
	}; 
	$out = $_.Name + ":" + $type; $out
}