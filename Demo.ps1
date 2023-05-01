$FILE = "demo.txt"

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
        [string]$File,
        [string]$Action
    )
    $FilePath = Split-Path $File -Parent
    $FileName = Split-Path $File -Leaf

    $global:FileChanged = $false
    $Watcher = New-Object IO.FileSystemWatcher $FilePath, $FileName -Property @{ 
        IncludeSubdirectories = $false
        EnableRaisingEvents = $true
    }
    $onChange = Register-ObjectEvent $Watcher Changed -Action {$global:FileChanged = $true}

    while ($global:FileChanged -eq $false){
        Start-Sleep -Milliseconds 100
    }

    Unregister-Event -SubscriptionId $onChange.Id
}


# Define input file
$INPUTFILE = "./out"


# Launch app
Start-Process -FilePath "./UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$PSScriptRoot/$FILE`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"`" -FullScreen -Debug" -NoNewWindow | Out-Null


# Demo Script

## Startup
UI "MainText --Text='Hello $($env:UserName)'"
UI "MainImage --Source='$PSScriptRoot/Windows_logo.png' --Height=200"
UI "Progress -Hide"
UI "SubText --Text='Let's setting up your device.'"
UI "Input --Type=Button --Button='Continue --Out=$INPUTFILE"
Wait-FileChange -File $INPUTFILE


## BitLocker
UI "MainText --Text='BitLocker setup"
UI "MainImage --Source='$PSScriptRoot/security.png' --Height=200"
UI "SubText --Text='Let's setting up BitLocker to protect your data.'"
UI "Progress --Type='Determinate' --Value=0 -ShowPercentage"

$PIN = $False;
$PINRE = $True;
while ($PIN -ne $PINRE) {

    UI "Progress --Type='Determinate' --Value=33 -ShowPercentage"

    UI "Input --Type=Password --Header='Type BitLocker password' --Button=Continue --Out=$INPUTFILE"
    Wait-FileChange -File $INPUTFILE
    $PIN = $(Get-Content -Path $INPUTFILE)
    Remove-Item $INPUTFILE

    UI "Progress --Type='Determinate' --Value=66 -ShowPercentage"
    Start-Sleep -Milliseconds 500 

    UI "Input --Type=Password --Header='Retype BitLocker password' --Button='Set PIN' --Out=$INPUTFILE"
    Wait-FileChange -File $INPUTFILE
    $PINRE = $(Get-Content -Path $INPUTFILE)
    Remove-Item $INPUTFILE
}


### Use the following code to setup BitLocker
###
### $BLV = Get-BitlockerVolume -MountPoint "C:"
### $TpmPinKeyProtector = $BLV.KeyProtector | Where-Object {$PSItem.KeyProtectorType -eq "TpmPin"}
### Remove-BitLockerKeyProtector -MountPoint "C:" -KeyProtectorId $TpmPinKeyProtector.KeyProtectorId
### Add-BitLockerKeyProtector -MountPoint $env:SystemDrive -Pin $(ConvertTo-SecureString $PIN -AsPlainText -Force) -TpmAndPinProtector

UI "Progress --Type='Determinate' --Value=100 -ShowPercentage"
UI "SubText --Text='BitLocker successfully setup.'" 

Start-Sleep -Seconds 1



## Choose Theme
UI "MainText --Text='Personalization"
UI "MainImage --Source='$PSScriptRoot/personalization.png' --Height=200"
UI "SubText --Text='Choose prefered theme'" 
UI "Progress -Hide"

UI "Input --Type=ImageChooser --Header='Select theme' --Value='$PSScriptRoot/WinDark.png|$PSScriptRoot/WinLight.jpg' --Button=Save --Out=$INPUTFILE"
Wait-FileChange -File $INPUTFILE
$THEME = $(Get-Content -Path $INPUTFILE)
Remove-Item $INPUTFILE


### Use the following code to setup theme
###



## Reboot
UI "MainText --Text='Finalization"
UI "MainImage --Source='$PSScriptRoot/restart.png' --Height=200"
UI "SubText --Text='Your device is ready to go but needs a restart, please wait.'"
UI "Progress --Type=Indeterminate"

Start-Sleep -Seconds  3
UI "Terminate"