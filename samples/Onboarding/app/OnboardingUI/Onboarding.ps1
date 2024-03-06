################################################################################
###################### Global functions and variables ##########################
################################################################################
Import-Module "$PSScriptRoot/modules/Invoke-AsCurrentUser"

$FILE = "$PSScriptRoot/demo.txt"

# Remove file if already exist
if (Test-Path $FILE) {
    Remove-Item $FILE | Out-Null
}

# Create file
New-Item $FILE | Out-Null

function UI {
    param(
        [Parameter(Position=0)]
        [string] $command
    )

    Write-Host($command)
    $command | out-file -append $FILE
}

function Wait-FileChange {
    param(
        [string]$File
    )
    $FilePath = Split-Path $File -Parent
    $FileName = Split-Path $File -Leaf

    $global:FileChanged = $false

    $Watcher = New-Object IO.FileSystemWatcher $FilePath, $FileName -Property @{ 
        IncludeSubdirectories = $false
        EnableRaisingEvents = $true
    }
    
    Unregister-Event -SourceIdentifier "filechanged" -ErrorAction SilentlyContinue

    Register-ObjectEvent $Watcher Changed -Action {$global:FileChanged = $true} -SourceIdentifier "filechanged" | Out-Null

    while ($global:FileChanged -eq $false){
        Start-Sleep -Milliseconds 100
    }

    Unregister-Event -SourceIdentifier "filechanged"
}

# Define input file
$INPUTFILE = "C:\Users\Public\out"


# Prevent taskbar and start menu to pop-up
Start-Job -Scriptblock {
    While($True){
        Get-Process StartMenuExperienceHost -ErrorAction SilentlyContinue | Stop-Process -Force
        Get-Process SetupHost -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Milliseconds  50
    }
    
}

################################################################################
############################### UI Launch ######################################
################################################################################

# Launch app
If($(whoami) -eq "nt authority\system"){
    Invoke-AsCurrentUser {
        Start-Process -FilePath "$($Argv[0])/UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$($Argv[1])`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"`" -AlwaysOnTop -FullScreen" -NoNewWindow | Out-Null
    } -Arguments @(,$PSScriptRoot,$FILE)
}Else{
    Start-Process -FilePath "$PSScriptRoot/UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$FILE`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"`" -AlwaysOnTop -FullScreen" -NoNewWindow -RedirectStandardOutput ".\NUL" | Out-Null
}


################################################################################
############################### TOU ############################################
################################################################################

UI "MainText --Text=`"Hello $(Get-WmiObject Win32_Process -Filter "Name='explorer.exe'" | ForEach-Object { $_.GetOwner() } | Select-Object -Unique)`""
UI "MainImage --Source=`"$PSScriptRoot/assets/terms.png`" --Height=150"
UI "Progress -Hide"
UI "SubText --Text=`"Please read and accept terms of use.`""
UI "Input --Type=ButtonText --Value=`"$($(Get-Content -Path "$PSScriptRoot/assets/TermOfUse.xaml").Replace('$PSScriptRoot',"$PSScriptRoot") )`" --Button=`"Accept`" --Height=300  --Width=400 --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE
Remove-Item $INPUTFILE

################################################################################
############################### TimeZone #######################################
################################################################################

UI "MainText --Text=`"Timezone`""
UI "MainImage --Source=`"$PSScriptRoot/assets/timezone.png`" --Height=150"
UI "Progress -Hide"
UI "SubText --Text=`"Please select your timezone.`""
UI "Input --Type=ComboBox --Value=`"$((Get-TimeZone).DisplayName)`" --AllowedValues=`"$(([System.TimeZoneInfo]::GetSystemTimeZones()).DisplayName -join "|")`" --Button=`"Save`" --Width=400  --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE
$TIMEZONE = $(Get-Content -Path $INPUTFILE)
Set-TimeZone -Id (([System.TimeZoneInfo]::GetSystemTimeZones()) | Where-Object {"$($_.DisplayName)" -eq "$TIMEZONE"}).Id
Remove-Item $INPUTFILE


################################################################################
############################### BitLocker PIN ##################################
################################################################################

UI "MainText --Text=`"BitLocker setup"
UI "MainImage --Source=`"$PSScriptRoot/assets/security.png`" --Height=150"
UI "SubText --Text=`"Let's setting up BitLocker to protect your data.`""

$PIN = $False;
$PINRE = $True;
while ($PIN -ne $PINRE) {


    UI "Input --Type=Password --Header=`"Type BitLocker password`" --Button=Continue --Width=400 --Out=`"$INPUTFILE`""
    Wait-FileChange -File $INPUTFILE
    $PIN = $(Get-Content -Path $INPUTFILE)
    Remove-Item $INPUTFILE


    UI "Input --Type=Password --Header=`"Retype BitLocker password`" --Button=`"Set PIN`" --Width=400 --Out=`"$INPUTFILE`""
    Wait-FileChange -File $INPUTFILE
    $PINRE = $(Get-Content -Path $INPUTFILE)
    Remove-Item $INPUTFILE
}


### Use the following code to setup BitLocker
###
#$BLV = Get-BitlockerVolume -MountPoint "C:"
#$TpmPinKeyProtector = $BLV.KeyProtector | Where-Object {$PSItem.KeyProtectorType -eq "TpmPin"}
#Remove-BitLockerKeyProtector -MountPoint "C:" -KeyProtectorId $TpmPinKeyProtector.KeyProtectorId
#Add-BitLockerKeyProtector -MountPoint $env:SystemDrive -Pin $(ConvertTo-SecureString $PIN -AsPlainText -Force) -TpmAndPinProtector

UI "Load --Text='Setting up BitLocker PIN...'"
Start-Sleep -Seconds 1 

UI "Load -Hide"
UI "SubText --Text=`"BitLocker successfully setup.`"" 

Start-Sleep -Seconds 1



################################################################################
############################### Personalization ################################
################################################################################



##################################### Taskbar ###################################



UI "MainText --Text=`"Personalization"
UI "MainImage --Source=`"$PSScriptRoot/assets/personalization.png`" --Height=150"
UI "SubText --Text=`"Choose your prefered taskbar position`"" 
UI "Progress -Hide"
UI "Input --Type=ImageChooser --Header=`"Select taskbar position`" --AllowedValues=`"$PSScriptRoot/assets/StartLeft.png#Left|$PSScriptRoot/assets/StartCenter.png#Center`" --Width=300 --Button=Save --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE

$POSITION = $(Get-Content -Path $INPUTFILE)
Remove-Item $INPUTFILE


## Apply theme
If($POSITION -eq "$PSScriptRoot/assets/StartLeft.png"){
    New-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "TaskbarAl" -Value 0 -Force
}Else{
    New-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "TaskbarAl" -Value 1 -Force
}



##################################### Theme ######################################


UI "MainText --Text=`"Personalization"
UI "MainImage --Source=`"$PSScriptRoot/assets/personalization.png`" --Height=150"
UI "SubText --Text=`"Choose your prefered theme`"" 
UI "Progress -Hide"
UI "Input --Type=ImageChooser --Header=`"Select theme`" --AllowedValues=`"$PSScriptRoot/assets/WinDark.png#Dark theme|$PSScriptRoot/assets/WinLight.jpg#Light theme`" --Width=300 --Button=Save --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE

UI "Load --Text='Applying theme...'"

$THEME = $(Get-Content -Path $INPUTFILE)
Remove-Item $INPUTFILE


### Use the following code to setup theme 

$THEME_FOLDER = "C:\Windows\resources\Themes"

## Apply theme
If($THEME -eq "$PSScriptRoot/assets/WinDark.png"){
    Invoke-AsCurrentUser {
        rundll32.exe themecpl.dll,OpenThemeAction "$($Argv[0])/dark.theme"
    } -Arguments @(,$THEME_FOLDER)
}Else{
    Invoke-AsCurrentUser {
        rundll32.exe themecpl.dll,OpenThemeAction "$($Argv[0])/aero.theme"
    } -Arguments @(,$THEME_FOLDER)
}

## Wait for process to start
$loop=0
While("$(Get-Process SystemSettings -ErrorAction SilentlyContinue)" -eq ""){
    Start-Sleep -Milliseconds  100
    $loop=+1
    If($loop -ge 50){
        Break
    }
}
Start-Sleep -Seconds 1

## Close process

$loop=0
While("$(Get-Process SystemSettings -ErrorAction SilentlyContinue)" -ne ""){
    Start-Sleep -Milliseconds  100
    $loop=+1
    If($loop -ge 50){
        Break
    }
    Get-Process SystemSettings -ErrorAction SilentlyContinue | Stop-Process -force
}



################################################################################
############################### Reboot #########################################
################################################################################
UI "MainText --Text=`"Finalization"
UI "MainImage --Source=`"$PSScriptRoot/assets/restart.png`" --Height=150"
UI "SubText --Text=`"Your device is ready to go but needs a restart, please wait.`""
UI "Load --Text='Waiting for reboot...'"

Start-Sleep -Seconds  3
UI "Terminate"
Restart-Computer -Force
