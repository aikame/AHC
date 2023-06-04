param([string] $userID)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
if ( $null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue) ) {
    if ( (Get-ADUser -identity $userID  | Select-Object -expand Enabled) -eq $true) {
        try {Disable-ADAccount -identity $userID
            return $true
        }catch {return $false} }
    } 
else {
    return $false
} 