param([string] $grpName, $Description)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$group = New-ADGroup -Name $grpName -GroupCategory Security -GroupScope Global -Description $Description
$result = Get-ADGroup $grpName -property Description | ConvertTo-Json
return $result