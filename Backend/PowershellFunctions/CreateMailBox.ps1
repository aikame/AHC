param([string] $userLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
if ( $null -ne $userInfo ) {
    try {
        Enable-Mailbox $UserLogin
        return $true
    } catch {
        return $false
    }              
} 
else {
    return $false
}
          
        
