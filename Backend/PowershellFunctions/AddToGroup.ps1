param([string] $UserLogin,$GroupLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$userInfo = Get-ADUser $UserLogin -ErrorAction Ignore
$groupInfo = Get-ADGroup $GroupLogin -ErrorAction Ignore
if ($null -ne $userInfo ) 
{
        if ($null -ne $groupInfo ) 
        {
            $members = (Get-ADGroupMember -Identity $GroupLogin -Recursive | Select-Object -ExpandProperty SamAccountName)
            if ( $members -notcontains $UserLogin) 
            {   
                try
                {
                    Add-ADGroupMember -identity $GroupLogin -Members $UserLogin -Confirm: $false
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
            return "403"
        }
} 
else 
{
    return "404"
}