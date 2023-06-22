Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
$User = "MYDOMAIN\Администратор"
$PWord = ConvertTo-SecureString -String "123" -AsPlainText -Force
$Credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $User, $PWord 
$exchSession = New-PSSession -ConfigurationName Microsoft.Exchange -ConnectionUri "http://WIN-HBSQBQLKH1H.MYDOMAIN.com/powershell"  -Authentication Kerberos -Credential $Credential
Import-PSSession $exchSession -DisableNameChecking