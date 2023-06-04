// Если я правильно понял, город и отдел выбирается в UI и хранится в бд, поэтому
// в скрипт они передаются уже сразу
param([string] $name,$surname,$midname,$city,$company,$department,$position)
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


    $extAttr1.Add('Город',$city)
    $extAttr1.Add('Компания',$company)
    $extAttr1.Add('Отдел',$department)
    $extAttr1.Add('Должность',$position)

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
        -GivenName $firstName  -Surname $lastName -OtherName $position-SamAccountName $userName 
        -City $city -Company $company -title $position -Enabled $true
                      
    Set-ADAccountPassword $UserName -Reset -NewPassword (ConvertTo-SecureString -AsPlainText “PASSWORD” -Force -Verbose) | Set-ADuser -ChangePasswordAtLogon $True

    Set-ADuser -Identity $userName -Add @{extensionAttribute1 = $extAttr1["Имя"]} 
    Set-ADuser -Identity $userName -Add @{extensionAttribute2 = $extAttr1["Фамилия"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute3 = $extAttr1["Отчество"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute4 = $extAttr1["Компания"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute5 = $extAttr1["Должность"]}
    Set-ADuser -Identity $userName -Add @{extensionAttribute6 = $extAttr1["Отдел"]}
    return 0
} else{
    return 1
}