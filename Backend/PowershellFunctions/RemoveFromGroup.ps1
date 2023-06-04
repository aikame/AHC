param([string] $grpID,$userID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ($null -ne (Get-ADGroup -Identity $grpID -ErrorAction SilentlyContinue)) {
    if ( $null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue )) {
        if ( $null -ne ((Get-ADUser $userID -Properties MemberOf).memberof -like "*$grpID*")) {
        try {
            Remove-ADGroupMember -identity $grpID -Members $userID
            return $true
        }
        catch {return $false}      
        } else {
            return $false   
        }
    } 
    else {
        return $false
    } 
}
else {
    return $false
}