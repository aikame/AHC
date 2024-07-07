param([string] $userLogin)

Add-PSSnapin Microsoft.Exchange.Management.PowerShell.SnapIn
$userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
if ( $null -ne $userInfo ) {
    try {
        Enable-Mailbox $UserLogin
	$mail = Get-Mailbox -Identity $UserLogin | Select-Object PrimarySmtpAddress | ConvertTo-Json
        return $mail
    } catch {
        return "400"
    }              
} 
else {
    return "404"
}
          
        
