param([string] $userLogin)

Add-PSSnapin Microsoft.Exchange.Management.PowerShell.SnapIn
$userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
if ( $null -ne $userInfo ) {
    try {
        Enable-Mailbox $UserLogin | Out-Null
	$mail = Get-Mailbox -Identity Dima.Dimovichev | Select-Object -ExpandProperty  PrimarySmtpAddress |Select-object address | Convertto-json
        return $mail
    } catch {
        return "400"
    }              
} 
else {
    return "404"
}
          
        
