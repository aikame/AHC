Set-ExecutionPolicy Unrestricted -Scope CurrentUser

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
        DeviceID = $d.DeviceID
        FreeSpaceGB = [math]::round($d.FreeSpace / 1GB, 2)
        TotalSpaceGB = [math]::round($d.Size / 1GB, 2)
    }
}

# Get CPU Info
$cpu = Get-CimInstance -ClassName Win32_Processor
$cpuName = $cpu.Name
$cpuCores = $cpu.NumberOfCores

# Get Computer Name
$computerName = $env:COMPUTERNAME
$role = Get-WmiObject -Class Win32_ComputerSystem | Select-Object -ExpandProperty DomainRole

# Output information
$info = [PSCustomObject]@{
    WindowsEdition = $windowsEdition
    IPAddress = $ipAddress
    DomainName = $domainName
    TotalRAMGB = $totalRAM
    DiskSpace = $diskSpace
    CPUName = $cpuName
    CPUCores = $cpuCores
    ComputerName = $computerName
    ComputerRole = $role
}

$jsonInfo = $info | ConvertTo-Json

# Save the information to a file
return $jsonInfo
