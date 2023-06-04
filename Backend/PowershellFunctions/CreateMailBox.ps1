param([string] $userLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
if ( $null -ne $userInfo ) {
    try {
        Enable-Mailbox $UserLogin
        return "200"
    } catch {
        return "400"
    }              
} 
else {
    return "404"
}
          
        
