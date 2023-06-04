param([string] $GroupID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$groupInfo = Get-ADGroup -identity $GroupID -ErrorAction ignore
if ( $null -ne $groupInfo ) {
    $members = $(Get-ADGroupMember $GroupID | Select-Object -expand Name)
    return $members
}
else {
    return $GroupID
}
