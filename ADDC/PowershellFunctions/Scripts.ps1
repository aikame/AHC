Set-ExecutionPolicy Unrestricted -Scope CurrentUser
# Добавление в группу
function AddToGroup {
    param([string] $grpID, $userID)
    if ($null -ne (Get-ADGroup -Identity $grpID -ErrorAction SilentlyContinue )) {
        if ( $null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue )) {
            $members = Get-ADGroupMember -Identity $grpID -Recursive | Select-Object -ExpandProperty SamAccountName
            if ( $members -notcontains $userID) {

                try {
                    Add-ADGroupMember -identity $grpID -Members $userID
                    return "200"
                }
                catch { return "400" }
            }
            else {
                return "404"   
            }
        } 
        else {
            return "404"
        } 
    }
    else {
        return "404"
    }
}
# Аутентификация
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
    }
    catch {
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
            }
            else {
                return $false
            }        
        }
        else {
            return $false
        }
    }
    else {
        Write-Output "Пользователь $username не найден"
    }
}

function Authentication {
    param (
        [string]$username,
        [string]$password
    )
    $groupName = "Администраторы AHC"
    $result = $false
    $checkCreds = Test-Credentials -username $username -password $password
    # Проверяем учетные данные пользователя
    if ($checkCreds) {
        # Проверяем членство в группе
        $result = Check-GroupMembership -username $username -groupName $groupName
    }
    if ($result) {
        return "200"
    }
    elseif ($checkCreds) {
        return "403"
    }
    else {
        return "401"
    }
}
# BAN
function BanUser {
    param([string] $UserLogin)
    if ( $null -ne (Get-ADUser -identity $UserLogin -ErrorAction SilentlyContinue) ) {
        if ( (Get-ADUser -identity $UserLogin  | Select-Object -expand Enabled) -eq $true) {
            try {
                Disable-ADAccount -identity $UserLogin
                return "200"
            }
            catch { return "400" } 
        }
        else {
            return "410"
        }
    } 
    else {
        return "404"
    } 
    
}
#Сменить пароль
function ChangePassw {
    param([string] $userID, $newPasswd)
    if ( $null -ne (Get-ADUser -identity $userID -ErrorAction SilentlyContinue ) ) {
        try {
            @( 
                Set-ADAccountPassword $userID -Reset -NewPassword (ConvertTo-SecureString -AsPlainText $newPasswd -Force -Verbose) -ErrorAction SilentlyContinue | Set-ADuser -ChangePasswordAtLogon $True )
            return "200"
        }
        catch { return "400" } 
    }
    else {
        return "404"
    }
    
}
# Собрать информацию о ПК
function CollectInfo {
    # Ensure the Active Directory module is available
    if (-not (Get-Module -ListAvailable -Name ActiveDirectory)) {
        Import-Module ActiveDirectory
    }

    # Get Windows Edition
    $os = Get-CimInstance -ClassName Win32_OperatingSystem
    $windowsEdition = $os.Caption

    # Get IP Address
    $ipAddress = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -like 'Ethernet*' -or $_.PrefixOrigin -eq 'Manual' } | Select-Object -ExpandProperty IPAddress -First 1)

    if (-not $ipAddress) {
        $ipAddress = "IP Address not found"
    }

    # Get Domain Name
    try {
        $domain = Get-ADDomain
        $domainName = $domain.DNSRoot
    }
    catch {
        $domainName = "Domain not found or not accessible"
    }

    # Get Total RAM
    $totalRAM = [math]::round($os.TotalVisibleMemorySize / 1MB, 2)

    # Get Disk Space
    $disk = Get-CimInstance -ClassName Win32_LogicalDisk -Filter "DriveType=3"
    $diskSpace = @()
    foreach ($d in $disk) {
        $diskSpace += [PSCustomObject]@{
            DeviceID     = $d.DeviceID
            FreeSpaceGB  = [math]::round($d.FreeSpace / 1GB, 2)
            TotalSpaceGB = [math]::round($d.Size / 1GB, 2)
        }
    }

    # Get CPU Info
    $cpu = Get-CimInstance -ClassName Win32_Processor
    $cpuName = @($cpu.Name)
    $cpuCores = @($cpu.NumberOfCores)

    # Get Computer Name
    $computerName = $env:COMPUTERNAME
    $role = Get-WmiObject -Class Win32_ComputerSystem | Select-Object -ExpandProperty DomainRole

    # Output information
    $info = [PSCustomObject]@{
        WindowsEdition = $windowsEdition
        IPAddress      = $ipAddress
        DomainName     = $domainName
        TotalRAMGB     = $totalRAM
        DiskSpace      = $diskSpace
        CPUName        = $cpuName
        CPUCores       = $cpuCores
        ComputerName   = $computerName
        ComputerRole   = $role
    }

    $jsonInfo = $info | ConvertTo-Json

    # Save the information to a file
    return $jsonInfo
}

# Создание группы
function CreateGroup {
    param([string] $grpName, $Description)
    $group = New-ADGroup -Name $grpName -GroupCategory Security -GroupScope Global -Description $Description
    $result = Get-ADGroup $grpName -property Description | ConvertTo-Json
    return $result
}


# Получение списка приложений
function GetAppInfo {
    $apps = Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Select-Object DisplayName, DisplayVersion, Publisher, InstallDate | sort-object displayname | Where-Object { $_.DisplayName -ne $null }

    $list = "{AppList: $($apps | ConvertTo-Json)}"
    return $list    
}

# Получение списка групп
function GetGroupInfo {
    param([string] $GroupID)
    $groupInfo = Get-ADGroup -identity $GroupID -ErrorAction ignore
    if ( $null -ne $groupInfo ) {
        $info = "
            Name: $($groupInfo  | Select-Object -expand SamAccountName) 
            Container: $($groupInfo  | Select-Object -expand DistinguishedName)  
            Type: $($groupInfo  | Select-Object -expand GroupCategory)  
            Group Scope: $($groupInfo  | Select-Object -expand GroupScope)"
        $members = "Members: " + (Get-ADGroupMember $GroupID | Select-Object -expand Name)
        $info = $info + $members
        return $info
    }
    else {
        return "404"
    }
}

# Получения списка пользователей в группе
function GetGroupMembers {
    param([string] $GroupID)

    $groupMembers = Get-ADGroupMember $GroupID | ForEach-Object {
        Get-ADUser -Identity $_.DistinguishedName -Properties Name, extensionAttribute1, extensionAttribute2, extensionAttribute3, Title | Select-Object Name, extensionAttribute1, extensionAttribute2, extensionAttribute3, Title
    }
    $list = "{Members: $($groupMembers | ConvertTo-Json)}"
    return $list
}

# Получение информации о пользователе
function GetUserInfo {
    param(
        [string] $UserLogin
    ) 
    Set-ExecutionPolicy Unrestricted -Scope CurrentUser
    $userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
    if ( $null -ne $userInfo ) {
        $info = Get-ADUser -identity $UserLogin -properties SamAccountName, EmailAddress, Enabled, PasswordExpired, PasswordLastSet, MemberOf | select-object ObjectGUID, DistinguishedName, SamAccountName, EmailAddress, Enabled, PasswordExpired, PasswordLastSet, MemberOf | ConvertTo-Json
        return $info
    }
    else {
        return "404"
    } 

}

# Скрыть почтовый ящик
function HideMailBox {
    param([string] $userLogin)
    if ($null -ne (Get-ADUser -identity $userLogin -ErrorAction SilentlyContinue) ) {
        try {
            Set-ADUser -Identity $userLogin -replace @{msExchHideFromAddressLists = $true }
            return "200"
        }
        catch {
            return "400"
        }
    }
    else {
        return "404"
    }
}

# Убрать из группы
function RemoveFromGroup {
    param([string] $grpLogin, $userLogin)
    if ( $null -ne (Get-ADUser -identity $userLogin -ErrorAction SilentlyContinue )) {
        if ( $null -ne ((Get-ADUser $userLogin -Properties MemberOf).memberof -like "*$grpLogin*")) {
            try {
                Remove-ADGroupMember -identity $grpLogin -Members $userLogin -Confirm:$false
                return "200"
            }
            catch {
                return "400"
            }      
        } 
        else {
            return "404"   
        }
    } 
    else {
        return "404"
    } 

}

# Показать почтовый ящик
function ShowMailBox {
    param([string] $userLogin)
    if ($null -ne (Get-ADUser -identity $userLogin -ErrorAction SilentlyContinue) ) {
        Set-ADUser -Identity $userLogin -replace @{msExchHideFromAddressLists = $false }
        return "200"
    }
    else {
        return "400"
    }
}

# Unban
function UnbanUser {
    param([string] $UserLogin)
    if ( $null -ne (Get-ADUser -identity $UserLogin -ErrorAction SilentlyContinue )) {
        if ( (Get-ADUser -identity $UserLogin  | Select-Object -expand Enabled) -eq $false) {
            try {
                Enable-ADAccount -identity $UserLogin
                return "200"
            }
            catch {
                return "500" 
            }
        } 
        else {
            return "410"  
        }
    }
    else {
        return "404"
    }
    
}

# Создание пользователя
function UserCreation {
    param([Parameter()]
        [string]$name,
        [Parameter()]
        [string]$surname,
        [Parameter()]
        [string]$midname,
        [Parameter()]
        [string]$city,
        [Parameter()]
        [string]$RUcity,
        [Parameter()]
        [string]$company,
        [Parameter()]
        [string]$RUcompany,
        [Parameter()]
        [string]$department,
        [Parameter()]
        [string]$RUdepartment,
        [Parameter()]
        [string]$appointment,
        [Parameter()]
        [string]$RUappointment,
        [Parameter()]
        [string]$password)


    if (($name -notmatch "[^А-Яа-яеЁ-]+") -and 
($surname -notmatch "[^А-Яа-яеЁ-]+") -and
($midname -notmatch "[^А-Яа-яеЁ-]+")) {
        $extAttr1 = @{}
        $extAttr1.Add('Имя', $name)
        $extAttr1.Add('Фамилия', $surname)
        $extAttr1.Add('Отчество', $midname)

        $firstName = &"PowershellFunctions\funcs\translit.ps1" $name
        $lastName = &"PowershellFunctions\funcs\translit.ps1" $surname
        $midName = &"PowershellFunctions\funcs\translit.ps1" $midname


        $extAttr1.Add('Город', $RUcity)
        $extAttr1.Add('Компания', $RUcompany)
        $extAttr1.Add('Отдел', $RUdepartment)
        $extAttr1.Add('Должность', $RUappointment)

        $index = 0  
        $baseUserName = "$firstName.$lastName" 
        while ($true) {    
            $userName = $baseUserName     
            if ($index -ne 0) {         
                $userName += $index     
            }     
            $user = Get-ADUser -identity $userName -ErrorAction SilentlyContinue     
            if ( $null -eq $user ) {                  
                break     
            }     
            $index++ 
        }

        $domain = [System.DirectoryServices.ActiveDirectory.Domain]::GetCurrentDomain().Name

        $UserPName = "$userName@$domain"

        New-ADUser -Name $UserName -UserPrincipalName $UserPName -Department $department -GivenName $firstName  -Surname $lastName -OtherName $midname -SamAccountName $userName -City $city -Company $company -title $appointment -Enabled $true
        $newuser = Get-ADUser -identity $UserName -ErrorAction SilentlyContinue

        $CurDir = pwd
        if ( $null -eq $newuser ) {                  
            return $CurDir   
        }                   
        Set-ADAccountPassword $userName -Reset -NewPassword (ConvertTo-SecureString -AsPlainText $password -Force -Verbose)
        Set-ADUser -Identity $userName -ChangePasswordAtLogon:$true 
        Set-ADuser -Identity $userName -Add @{extensionAttribute1 = $extAttr1["Имя"] } 
        Set-ADuser -Identity $userName -Add @{extensionAttribute2 = $extAttr1["Фамилия"] }
        Set-ADuser -Identity $userName -Add @{extensionAttribute3 = $extAttr1["Отчество"] }
        Set-ADuser -Identity $userName -Add @{extensionAttribute4 = $extAttr1["Компания"] }
        Set-ADuser -Identity $userName -Add @{extensionAttribute5 = $extAttr1["Должность"] }
        Set-ADuser -Identity $userName -Add @{extensionAttribute6 = $extAttr1["Отдел"] }
        $result = GetUserInfo -UserLogin $userName
        return $result
    }
    else {
        return "400"
    }
}