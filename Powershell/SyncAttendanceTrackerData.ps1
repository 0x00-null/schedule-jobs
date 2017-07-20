# Run weekly to call endpoint to load attendance tracker data
$apiPrefix = [environment]::GetEnvironmentVariable(<YOUR ENDPOINT ENV VAR GOES HERE>, "Machine")
Invoke-WebRequest -Uri ($apiPrefix + <YOUR ENDPOINT GOES HERE>) -Method POST
