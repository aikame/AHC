param([string] $userLogin,$GroupLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ( $null -ne (Get-ADGroup -identity $GroupLogin -ErrorAction SilentlyContinue )) {
        if ( $null -ne ((Get-ADUser $userLogin -Properties MemberOf).memberof -like "*$GroupLogin*")) {
            try 
            {
                Remove-ADGroupMember -identity $GroupLogin -Members $userLogin -Confirm: $false
                return "200"
            }
            catch 
            {
                return "400"
            }      
        } 
        else 
        {
            return "500"   
        }
    }
else 
{
    return "404"
} 
