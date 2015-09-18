# Run nightly to call endpoint to remove expired corkboard posts from Amazon search index
$apiPrefix = [environment]::GetEnvironmentVariable("CRDS_CORKBOARD_API_ENDPOINT", "Machine")
Invoke-WebRequest -Uri ($apiPrefix + "api/syncposts") -Method POST
