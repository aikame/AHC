    param(
        [string] $UserLogin
    ) 
    Set-ExecutionPolicy Unrestricted -Scope CurrentUser
        $userInfo = Get-ADUser -identity $UserLogin -ErrorAction Ignore
            if ( $null -ne $userInfo ) {
            $info = Get-ADUser -identity $UserLogin -properties SamAccountName,EmailAddress,Enabled,PasswordExpired,PasswordLastSet,MemberOf | select-object DistinguishedName,SamAccountName,EmailAddress,Enabled,PasswordExpired,PasswordLastSet,MemberOf | ConvertTo-Json
            return $info
  }
  else {
    return $UserLogin
       } 
