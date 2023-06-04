param([string] $grpID,$userID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ($null -ne (Get-ADGroup -Identity $grpID -ErrorAction SilentlyContinue )) {
    if ( $null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue )) {
        $members = Get-ADGroupMember -Identity $grpID -Recursive | Select-Object -ExpandProperty SamAccountName
        if ( $members -notcontains $userID) {

        try {Add-ADGroupMember -identity $grpID -Members $userID
            return "200"
        }
        catch {return "400"}
        } else {
            return "404"   
        }
    } 
    else {
        return "404"
    } 
}
else {
    return "404"
}