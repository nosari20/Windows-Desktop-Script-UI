
# Onboarding

![Screenshot](/samples//Onboarding/Onboarding.gif)

## Step 1 : Prerequisites

### Windows App SDK

0. Download [Windows App SDK 1.5 ](https://learn.microsoft.com/fr-fr/windows/apps/windows-app-sdk/downloads#windows-app-sdk-15)
1. Package Windows App SDK file using Microsoft Win32 Content Prep Tool
2. Add it as Win32 app

Install comand:
```ps1
windowsappruntimeinstall-x64.exe
```

Detection:
```ps1
(get-appxpackage micro*win*appruntime*).packagefullname | Select-String -Pattern ".*1\.5.*"
```

Important: this app must be installed when user profile is created, so use a user group for assignment.


### .NET 8 (only for framework dependent version)

0. Download [.NET 8](https://dotnet.microsoft.com/en-us/download)
1. Package .NET 8 file using Microsoft Win32 Content Prep Tool
2. Add it as Win32 app

Install comand:
```ps1
dotnet-runtime-8.0.5-win-x64.exe /q /norestart
```

Detection:
```ps1
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User") 
Try{$(dotnet --list-runtimes) -like "Microsoft.NETCore.App 8.*"}Catch{}
```

## Step 2: Onboarding app

### Important considerations

In order to launch the UI right after ESP the following conditions must be met:
* App must be installed during ESP (block device unitil installation)
* App must include an autolaunch system (asynchronous start to prevent ESP blocking) -> see appendices for my implementation
* Include system to execute UI binary in user context (≠ system context) -> see appendices for my implementation
* The process ``StartMenuExperienceHost`` must be stopped to prevent Start Menu and Taskbar to appear after ESP (must be killed periodicaly because it is relaunched each time) -> see appendices for my implementation

### App directory structure sample


```
/
│   Launch-Onboarding.ps1 (copy OnboardingUI content to local directory and schedule autolaunch)
│
└───modules
│    │
│    └─── Invoke-AfterESP
│           Invoke-AfterESP.psm1
│
└─── OnboardingUI
    │    Onboarding.ps1 (launch and control UI)
    │     
    └─── modules
    │    │
    │    └─── Invoke-AsUser
    │            Invoke-AsUser.psm1   
    └─── assets (all assets used by UI) 
    │
    └─── UI (all UI binary files)    
```

### Deployment

1. Package app directory using Microsoft Win32 Content Prep Tool
2. Add it as Win32 app

Install comand:
```ps1
C:\Windows\Sysnative\WindowsPowerShell\v1.0\powershell.exe -ex bypass -file  Launch-Onboarding.ps1 #ensure 64bit PowerShell
```

Detection: 
```
Folder exists : C:\Program Files\Onboarding
```
Detection can be improved and match what information is asked to users (e.g. BitLocker PIN)

### Behavior

1. Files contained in ``OnboardingUI`` are copied to ``C:\Programs\Onboarding``
2. Scheduled task is created and is planed to be launched 10 seconds after creation. This task will launch ``Onboarding.ps1``
3. During initial setup (ESP, Windows Hello setup, etc.), the UI is not at the top of window stack
4. After initial setup, user see the UI



# Appendices

## Invoke-AfterESP PowerShell module

### Module
```ps1
Function Invoke-AfterESP(){

    param(
        [Parameter(Mandatory=$true, Position=0)] 
        [ValidateNotNullOrEmpty()]
        [Scriptblock]$ScriptBlock,

        [Parameter(Position=1)] 
        [string[]]$Arguments,

        [Parameter(Position=2)] 
        [string]$TaskName = "Invoke-AfterESP-$(Get-Random -Maximum 1000)"
    )

    # Create arguments
    $Argv ="`$Argv = @("
    If($Arguments){
        ForEach($Argument in $Arguments){
            $Type = "String"
            $Argument
            Try{
                $Type = ([int]$Argument).GetType().Name
            }Catch{}
            $Type
            If($Type -eq 'Int32'){
                $Argv += ",$Argument"
            }Else{
                $Argv += ",'$Argument'"
            }      
        }
    }
    $Argv += ")`n"

    # Put code in temp file
    ($Argv + $ScriptBlock.ToString()) | Out-File -FilePath "C:\Users\Public\$taskName.ps1"
    "Remove-Item -Path 'C:\Users\Public\$taskName.ps1';Unregister-ScheduledTask -TaskName '$taskName' -Confirm:`$false" | Out-File -FilePath "C:\Users\Public\$taskName.ps1" -Append

    # Define action
    $action = New-ScheduledTaskAction -Execute "powershell.exe" "-WindowStyle Hidden -ExecutionPolicy Bypass -File C:\Users\Public\$taskName.ps1"

    # Define trigger to run the task after logon
    $trigger = New-ScheduledTaskTrigger -Once -At ((Get-date).AddSeconds(10))

    # Define targeted user
    $principal = New-ScheduledTaskPrincipal -User "NT AUTHORITY\SYSTEM" -LogonType ServiceAccount
    
    # Allow start if on battery
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries

    # Create task
    $task = New-ScheduledTask -Action $action -Trigger $trigger -Principal $principal -Settings $settings
    
    # Register task
    Register-ScheduledTask $taskName -InputObject $task | Out-Null

    # Start Task
    #Start-ScheduledTask -TaskName $taskName
    
}

Export-ModuleMember -Function Invoke-AfterESP

```

### Usage

```ps1
$MyVar1 = 1
$MyVar2 = "lorem"

Invoke-AfterESP -ScriptBlock {
    Write-Host $($Argv[0])
    Write-Host $($Argv[1])
} -Arguments @($MyVar1, $MyVar2) -TaskName "MyTask"
```



## Invoke-AsCurrentUser PowerShell module

### Module
```ps1
Function Invoke-AsCurrentUser(){

    param(
        [Parameter(Mandatory=$true, Position=0)] 
        [ValidateNotNullOrEmpty()]
        [Scriptblock]$ScriptBlock,

        [Parameter(Position=1)] 
        [string[]]$Arguments
    )

    # Create arguments
    $Argv ="`$Argv = @("
    If($Arguments){
        ForEach($Argument in $Arguments){
            $Type = "String"
            $Argument
            Try{
                $Type = ([int]$Argument).GetType().Name
            }Catch{}
            $Type
            If($Type -eq 'Int32'){
                $Argv += ",$Argument"
            }Else{
                $Argv += ",'$Argument'"
            }      
        }
    }
    $Argv += ")`n"

    # Create random name for task
    $taskName="Invoke-AsCurrentUser-$(Get-Random -Maximum 1000)"

    # Put code in temp file
    ($Argv + $ScriptBlock.ToString()) | Out-File -FilePath "C:\Users\Public\$taskName.ps1"
    "Remove-Item -Path 'C:\Users\Public\$taskName.ps1'" | Out-File -FilePath "C:\Users\Public\$taskName.ps1" -Append

    # Define action
    $action = New-ScheduledTaskAction -Execute "powershell.exe" "-WindowStyle Hidden -ExecutionPolicy Bypass -File C:\Users\Public\$taskName.ps1"

    # Define trigger
    $trigger = New-ScheduledTaskTrigger -AtLogon

    # Define targeted user
    $principal = New-ScheduledTaskPrincipal -UserId (Get-CimInstance -ClassName Win32_ComputerSystem | Select-Object -expand UserName)
    
    # Define settings
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries

    # Create task
    $task = New-ScheduledTask -Action $action -Trigger $trigger -Principal $principal -Settings $settings
    
    # Register task
    Register-ScheduledTask $taskName -InputObject $task | Out-Null

    # Start Task
    Start-ScheduledTask -TaskName $taskName

    # Unregister Task
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}
Export-ModuleMember -Function Invoke-AsCurrentUser
```

### Usage

```ps1
$MyVar1 = 1
$MyVar2 = "lorem"

Invoke-AsCurrentUser -ScriptBlock {
    Write-Host $($Argv[0])
    Write-Host $($Argv[1])
} -Arguments @($MyVar1, $MyVar2) -TaskName "MyTask"
```
## Prevent Start menu pop-up after ESP
```ps1
Start-Job -Scriptblock {
    While($True){
        Get-Process StartMenuExperienceHost -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Milliseconds  100
    }
    
}
```



