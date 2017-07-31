# Run daily to call endpoint to load attendance tracker data
$previousDay = (Get-Date).AddDays(-1).ToString('MM-dd-yyyy')
$apiPrefix = [environment]::GetEnvironmentVariable("CRDS_GATEWAY_SERVER_ENDPOINT", "Machine")
Invoke-WebRequest -Uri ($apiPrefix + "Event/api/attendance?date=" + $previousDay) -Method POST