param([string] $userID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ( $null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue )) {
    if ( (Get-ADUser -identity $userID  | Select-Object -expand Enabled) -eq $false) {
    try {Enable-ADAccount -identity $userID
        return $true}
    catch {retutn $false }
    } else {
        retutn $false  
    }
}
else {
    return $false
}