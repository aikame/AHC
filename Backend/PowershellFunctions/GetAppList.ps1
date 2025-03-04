Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$apps = Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Select-Object DisplayName, DisplayVersion, Publisher, Size, InstallDate | sort-object displayname | Where-Object {$_.DisplayName -ne $null }

$list = "{AppList: $($apps | ConvertTo-Json)}"
return $list
