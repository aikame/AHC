param([string] $GroupLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$groupInfo = Get-ADGroup -identity $GroupLogin -ErrorAction ignore
if ( $null -ne $groupInfo ) {
    $members = Get-ADGroupMember $GroupLogin | Select-Object -expand Name
    $info = $groupInfo | Add-Member -MemberType NoteProperty -Name "Members" -Value $members -PassThru -Force | Select-Object  SamAccountName, DistinguishedName,GroupCategory,GroupScope,Members |ConvertTo-Json
    return $info
}
else {
    return "404"
}