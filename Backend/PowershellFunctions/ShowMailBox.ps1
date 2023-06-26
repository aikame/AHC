param([string] $userLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ($null -ne (Get-ADUser -identity $userLogin -ErrorAction SilentlyContinue) ) {
    Set-ADUser -Identity $userLogin -replace @{msExchHideFromAddressLists=$false}
    return "200"
} else {
    return "400"
}