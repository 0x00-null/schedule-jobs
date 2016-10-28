# Create a startup shortcut for all users on a Windows machine
$path = "Program Files (x86)\Google\Chrome\Application\chrome.exe"
$TargetFile = $env + $path
$ShortcutFile = "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp\Chrome.lnk"
$WScriptShell = New-Object -ComObject WScript.Shell
$Shortcut = $WScriptShell.CreateShortcut($ShortcutFile)
$Shortcut.TargetPath = $TargetFile
$Shortcut.Arguments = "https://echeck.crossroads.net/setup?machine=kiosk_guid_goes_here"
$Shortcut.Save()