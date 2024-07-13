param(
[string] $userLogin,
[string] $groupName
)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser

$user = Get-ADUser -Identity $userLogin -Properties MemberOf -ErrorAction SilentlyContinue
if ($user) {
    $group = Get-ADGroup -Identity $groupName -ErrorAction SilentlyContinue

    if ($group) {
        if ($user.MemberOf -contains $group.DistinguishedName) {
            return $true
        } else {
            return $false
        }
    } else {
        return "500"
    }
} else {
    return "404"
}