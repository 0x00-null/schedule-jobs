# Run daily to sync Pushpay settlements with MP deposits
$apiPrefix = [environment]::GetEnvironmentVariable("CRDS_GATEWAY_SERVER_ENDPOINT", "Machine")
$financeMicroserviceEndpoint = $apiPrefix + "finance/"
Invoke-WebRequest -Uri ($financeMicroserviceEndpoint + "api/deposit/sync") -Method POST

