param([string] $userLogin)

Add-PSSnapin Microsoft.Exchange.Management.PowerShell.SnapIn
$userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
if ( $null -ne $userInfo ) {
    try {
        Enable-Mailbox $UserLogin | Out-Null
	$mail = Get-Mailbox -Identity $userLogin | Select-Object -ExpandProperty  PrimarySmtpAddress |Select-object address | Convertto-json
        return $mail
    } catch {
        return $_.Exception
    }              
} 
else {
    return "404"
}
          
        
