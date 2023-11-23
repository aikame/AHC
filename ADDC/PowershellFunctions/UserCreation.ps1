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
     [string]$appointment),
     [Parameter()]
     [string]$RUappointment)
Set-ExecutionPolicy Unrestricted -Scope CurrentUser

if (($name -notmatch "[^А-Яа-яеЁ-]+") -and 
($surname -notmatch "[^А-Яа-яеЁ-]+") -and
($midname -notmatch "[^А-Яа-яеЁ-]+")){
    $extAttr1=@{}
    $extAttr1.Add('Имя',$firstName)
    $extAttr1.Add('Фамилия',$lastName)
    $extAttr1.Add('Отчество',$midName)

    $firstName = &"$PSScriptRoot\funcs\translit.ps1" $firstName
    $lastName = &"$PSScriptRoot\funcs\translit.ps1" $lastName
    $midName = &"$PSScriptRoot\funcs\translit.ps1" $midName


    $extAttr1.Add('Город',$RUcity)
    $extAttr1.Add('Компания',$RUcompany)
    $extAttr1.Add('Отдел',$RUdepartment)
    $extAttr1.Add('Должность',$RUappointment)

    $index = 0  
    $baseUserName = "$firstName.$lastName" 
    while ($true) {    
      $userName = $baseUserName     
      if ($index -ne 0) {         
        $userName += $index     
      }     
      $user =  Get-ADUser -identity $userName -ErrorAction SilentlyContinue     
      if ( $null -eq $user ) {                  
        break     
      }     
      $index++ 
    }

    $domain = [System.DirectoryServices.ActiveDirectory.Domain]::GetCurrentDomain().Name

    $UserPName = "$userName@$domain"

    New-ADUser -Name $UserName -UserPrincipalName $UserPName -Department $department 
        -GivenName $firstName  -Surname $lastName -OtherName $midname -SamAccountName $userName 
        -City $city -Company $company -title $appointment -Enabled $true
                      
    Set-ADAccountPassword $UserName -Reset -NewPassword (ConvertTo-SecureString -AsPlainText “PASSWORD” -Force -Verbose) | Set-ADuser -ChangePasswordAtLogon $True

    Set-ADuser -Identity $userName -Add @{extensionAttribute1 = $extAttr1["Имя"]} 
    Set-ADuser -Identity $userName -Add @{extensionAttribute2 = $extAttr1["Фамилия"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute3 = $extAttr1["Отчество"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute4 = $extAttr1["Компания"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute5 = $extAttr1["Должность"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute6 = $extAttr1["Отдел"]}
    return "200"
} else{
    return "400"
}