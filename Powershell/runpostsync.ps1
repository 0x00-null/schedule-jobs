# Run nightly to call endpoint to remove expired corkboard posts from Amazon search index
Invoke-WebRequest -Uri http://localhost:65341/api/syncposts/ -Method POST
