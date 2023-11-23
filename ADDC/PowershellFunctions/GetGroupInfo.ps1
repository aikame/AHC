param([string] $GroupID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$groupInfo = Get-ADGroup -identity $GroupID -ErrorAction ignore
if ( $null -ne $groupInfo ) {
    $info = "
            Name: $($groupInfo  | Select-Object -expand SamAccountName) 
            Container: $($groupInfo  | Select-Object -expand DistinguishedName)  
            Type: $($groupInfo  | Select-Object -expand GroupCategory)  
            Group Scope: $($groupInfo  | Select-Object -expand GroupScope)"
            $members = "Members: " + (Get-ADGroupMember $GroupLogin.Text | Select-Object -expand Name)
            $info =  $info + $members
    return $info
}
else {
    return "404"
}