param([string] $GroupLogin,$UserLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
if ($null -ne (Get-ADGroup -Identity $GroupLogin -ErrorAction SilentlyContinue )) {

        $members = Get-ADGroupMember -Identity $GroupLogin -Recursive | Select-Object -ExpandProperty SamAccountName
        if ( $members -notcontains $UserLogin) {

        try {Add-ADGroupMember -identity $GroupLogin -Members $UserLogin
            return "200"
        }
        catch {return "400"}
        } 
        else {
            return "500"   
        }
    } 
else {
    return "404"
}