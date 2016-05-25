Param(
	[string]$toEmail,
	[string]$fromEmail,
	[string]$subject,
	[string]$body,
	[string]$eventData
)

$finalBody = ([string]$body + [string]$eventData)

$smtpServer = [environment]::GetEnvironmentVariable("SMTP_SERVER", "Machine")
$smtpUsername = [environment]::GetEnvironmentVariable("SMTP_USERNAME", "Machine")
$smtpPassword = [environment]::GetEnvironmentVariable("SMTP_PASSWORD", "Machine")

$securePassword = ConvertTo-SecureString $smtpPassword -AsPlainText -Force # Done in order to pass this as a param
$credential = New-Object System.Management.Automation.PSCredential $smtpUsername, $securePassword

Send-MailMessage -smtpServer $smtpServer -Credential $credential -Usessl -Port 587 -from $fromEmail -to $toEmail -subject $subject -Body $finalBody
