Set-ExecutionPolicy Unrestricted -Scope CurrentUser
Add-PSSnapin Microsoft.Exchange.Management.PowerShell.SnapIn

#Создание почты
function CreateMailBox {
    param([string] $userLogin)

    $userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
    if ( $null -ne $userInfo ) {
        try {
            Enable-Mailbox $UserLogin | Out-Null
            $mail = Get-Mailbox -Identity $userLogin | Select-Object -ExpandProperty  PrimarySmtpAddress | Select-object address | Convertto-json
            return $mail
        }
        catch {
            return $_.Exception
        }              
    } 
    else {
        return "404"
    }
}