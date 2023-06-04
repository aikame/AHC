param([string] $userID,$newPasswd)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ( $null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue ) ) {
    try {@( Set-ADAccountPassword $UserName -Reset -NewPassword (ConvertTo-SecureString -AsPlainText $newPasswd -Force -Verbose) -ErrorAction SilentlyContinue | Set-ADuser -ChangePasswordAtLogon $True )
        return "200"}
    catch {return "400"} }
else {
    return "404"
}