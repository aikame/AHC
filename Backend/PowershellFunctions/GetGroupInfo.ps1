param([string] $GroupID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$groupInfo = Get-ADGroup -identity $GroupID -ErrorAction ignore
if ( $null -ne $groupInfo ) {
    $group = Get-ADGroup -identity $GroupID  | select-object SamAccountName,DistinguishedName,GroupCategory,GroupScope
    return $group
}
else {
    return $GroupID
}