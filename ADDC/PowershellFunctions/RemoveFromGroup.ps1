param([string] $grpLogin,$userLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ( $null -ne (Get-ADUser -identity $userLogin -ErrorAction SilentlyContinue )) {
    if ( $null -ne ((Get-ADUser $userLogin -Properties MemberOf).memberof -like "*$grpLogin*")) {
        try 
        {
            Remove-ADGroupMember -identity $grpLogin -Members $userLogin -Confirm:$false
            return "200"
        }
        catch 
        {
            return "400"
        }      
    } 
    else 
    {
        return "404"   
    }
} 
else 
{
    return "404"
} 
