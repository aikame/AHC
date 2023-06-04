param([string] $userID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ($null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue) ) {
    Set-ADUser -Identity $userID -replace @{msExchHideFromAddressLists=$true}
    return $true
} else {
    return $false
}