# BitLocker PIN

![Screenshot](/samples/BitLockerPIN/demo.gif)

## Step 1 : Prerequisites

### Windows App SDK

0. Download [Windows App SDK 1.4 ](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
1. Package Windows App SDK file using Microsoft Win32 Content Prep Tool
2. Add it as Win32 app

Install comand:
```ps1
windowsappruntimeinstall-x64.exe
```

Detection:
```ps1
(get-appxpackage micro*win*appruntime*).packagefullname | Select-String -Pattern ".*1\.4.*"
```

Important: this app must be installed when user profile is created, so use a user group for assignment.


### .NET 6 (only for framework dependent version)

0. Download [.NET 6](https://dotnet.microsoft.com/en-us/download)
1. Package .NET 6 file using Microsoft Win32 Content Prep Tool
2. Add it as Win32 app

Install comand:
```ps1
windowsappruntimeinstall-x64.exe /q /norestart
```

Detection:
```ps1
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User") 
Try{$(dotnet --list-runtimes) -like "Microsoft.NETCore.App 6.*"}Catch{}
```


## Step 2: BitLocker PIN UI

1. Package app directory using Microsoft Win32 Content Prep Tool
2. Add it as Win32 app

Install comand:
```ps1
C:\Windows\Sysnative\WindowsPowerShell\v1.0\powershell.exe -ex bypass -file  BitLockerPIN.ps1 #ensure 64bit PowerShell
```

Detection (allow app reinstallation when PIN recovery key is used): 
```ps1
$latestPINChangeDate = $(Get-WinEvent -FilterHashtable @{logname="Microsoft-Windows-BitLocker/BitLocker Management"} | Where-Object -Property Id -Eq 776)[0].TimeCreated

$recoveryUsed = -Not [string]::IsNullOrEmpty($($(Get-WinEvent -FilterHashtable @{LogName='System'} | Where-Object -Property Message -Match 'BitLocker')[0] | Where-Object -Property Message -Match 'PIN'))

if($recoveryUsed){

    $bitlockerEventFound = -Not [string]::IsNullOrEmpty($(Get-WinEvent -FilterHashtable @{LogName='System'} | Where-Object -Property Message -Match 'BitLocker'))

    if($bitlockerEventFound){

        $latestRecoveryDate = $($(Get-WinEvent -FilterHashtable @{LogName='System'} | Where-Object -Property Message -Match 'BitLocker')[0] | Where-Object -Property Message -Match 'PIN').TimeCreated

        if(-Not $($latestPINChangeDate -gt $latestRecoveryDate)) {
            exit 1
        }
    }  
}

$bitlockerPINEnabled = -Not [string]::IsNullOrEmpty($($(Get-BitLockerVolume -MountPoint $env:SystemDrive).KeyProtector | Where { $_.KeyProtectorType -eq 'TpmPin' }))

if($bitlockerPINEnabled) {
    Write-Output "TPM + PIN Enabled"  
    exit 0 
}

exit 1
```

### Behavior

1. When app is in the installation process, user is prompted to setup BitLocker PIN
2. App is successfully installed when detectetion found that a PIN has bin setup (if recovery key is used, detection will fail and user will be able to relaunch the process)


