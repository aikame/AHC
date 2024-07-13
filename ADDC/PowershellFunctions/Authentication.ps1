param (
    [string]$username,
    [string]$password
)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
$groupName = "Администраторы AHC"
function Test-Credentials {
    param (
        [string]$username,
        [string]$password
    )

    $securePassword = ConvertTo-SecureString $password -AsPlainText -Force
    $credential = New-Object System.Management.Automation.PSCredential($username, $securePassword)

    try {
        Get-ADUser -Identity $username -Credential $credential -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

function Check-GroupMembership {
    param (
        [string]$username,
        [string]$groupName
    )

    $user = Get-ADUser -Identity $username -Properties MemberOf -ErrorAction SilentlyContinue

    if ($user) {
        $group = Get-ADGroup -Identity $groupName -ErrorAction SilentlyContinue

    	if ($group) {
        	if ($user.MemberOf -contains $group.DistinguishedName) {
           		return $true
        	} else {
            		return $false
		}        
    	} else {
        	return $false
    	}
    } else {
        Write-Output "Пользователь $username не найден"
    }
}

# Проверяем учетные данные пользователя
if (Test-Credentials -username $username -password $password) {
    # Проверяем членство в группе
    Check-GroupMembership -username $username -groupName $groupName
} else {
    Write-Output "Неправильное имя пользователя или пароль"
}