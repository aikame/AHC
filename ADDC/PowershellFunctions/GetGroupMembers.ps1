param([string] $GroupID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser

$groupMembers = Get-ADGroupMember $GroupID | ForEach-Object {
    Get-ADUser -Identity $_.DistinguishedName -Properties Name, extensionAttribute1, extensionAttribute2, extensionAttribute3, Title | Select-Object Name, extensionAttribute1, extensionAttribute2, extensionAttribute3, Title
}
$list = "{Members: $($groupMembers | ConvertTo-Json)}"
return $list