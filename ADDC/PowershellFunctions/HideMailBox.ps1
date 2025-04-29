param([string] $userLogin)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ($null -ne (Get-ADUser -identity $userLogin -ErrorAction SilentlyContinue) ) {
    try {
        Set-ADUser -Identity $userLogin -replace @{msExchHideFromAddressLists=$true}
        return "200"
    }
    catch {
        return "400"
    }
} else {
    return "404"
}