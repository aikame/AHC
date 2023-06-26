param([string] $UserLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ( $null -ne (Get-ADUser -identity $UserLogin -ErrorAction SilentlyContinue )) {
    if ( (Get-ADUser -identity $UserLogin  | Select-Object -expand Enabled) -eq $false) {
        try 
        {
            Enable-ADAccount -identity $UserLogin
            return "200"
        }
        catch 
        {
            return "500" 
        }
    } 
    else 
    {
        return "410"  
    }
}
else 
{
    return "404"
}