################################################################################
###################### Global functions and variables ##########################
################################################################################

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
$INPUTFILE = "$PSScriptRoot/out"



################################################################################
############################### UI Launch ######################################
################################################################################

# Launch app
Start-Process -FilePath "./UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$PSScriptRoot/$FILE`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"`" -AlwaysOnTop -FullScreen -Debug" -NoNewWindow | Out-Null


################################################################################
############################### Startup UI #####################################
################################################################################

UI "MainText --Text=`"Hello $($env:UserName)`""
UI "MainImage --Source=`"$PSScriptRoot/windows.png`" --Height=150"
UI "Progress -Hide"
UI "SubText --Text=`"Welcome to Windows, let's setup your device.`""
UI "Input --Type=ButtonVideo --Value=`"$PSScriptRoot/Windows11.mp4`" --Button=`"Continue`" --Height=300 --Width=500 -Autoplay --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE



################################################################################
############################### TOU ############################################
################################################################################

UI "MainText --Text=`"Terme of Use`""
UI "MainImage --Source=`"$PSScriptRoot/terms.png`" --Height=150"
UI "Progress -Hide"
UI "SubText --Text=`"Please read and accept terms of use.`""
UI "Input --Type=ButtonText --Value=`"$($(Get-Content -Path "$PSScriptRoot/TermOfUse.xaml").Replace('$PSScriptRoot',"$PSScriptRoot") )`" --Button=`"Accept`" --Height=300  --Width=400 --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE
Remove-Item $INPUTFILE

################################################################################
############################### TimeZone #######################################
################################################################################

UI "MainText --Text=`"Timezone`""
UI "MainImage --Source=`"$PSScriptRoot/timezone.png`" --Height=150"
UI "Progress -Hide"
UI "SubText --Text=`"Please select your timezone.`""
UI "Input --Type=ComboBox --Value=`"$((Get-TimeZone).DisplayName)`" --AllowedValues=`"$(([System.TimeZoneInfo]::GetSystemTimeZones()).DisplayName -join "|")`" --Button=`"Save`" --Width=400  --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE
$TIMEZONE = $(Get-Content -Path $INPUTFILE)
Remove-Item $INPUTFILE


################################################################################
############################### BitLocker PIN ##################################
################################################################################

UI "MainText --Text=`"BitLocker setup"
UI "MainImage --Source=`"$PSScriptRoot/security.png`" --Height=150"
UI "SubText --Text=`"Let's setting up BitLocker to protect your data.`""
UI "Progress --Type=`"Determinate`" --Value=0 -ShowPercentage"

$PIN = $False;
$PINRE = $True;
while ($PIN -ne $PINRE) {

    UI "Progress --Type=`"Determinate`" --Value=33 -ShowPercentage"

    UI "Input --Type=Password --Header=`"Type BitLocker password`" --Button=Continue --Width=400 --Out=`"$INPUTFILE`""
    Wait-FileChange -File $INPUTFILE
    $PIN = $(Get-Content -Path $INPUTFILE)
    Remove-Item $INPUTFILE

    UI "Progress --Type=`"Determinate`" --Value=66 -ShowPercentage"
    Start-Sleep -Milliseconds 500 

    UI "Input --Type=Password --Header=`"Retype BitLocker password`" --Button=`"Set PIN`" --Width=400 --Out=`"$INPUTFILE`""
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

UI "Progress --Type=`"Determinate`" --Value=100 -ShowPercentage"
UI "SubText --Text=`"BitLocker successfully setup.`"" 

Start-Sleep -Seconds 1


################################################################################
############################### Personalization ################################
################################################################################

UI "MainText --Text=`"Personalization"
UI "MainImage --Source=`"$PSScriptRoot/personalization.png`" --Height=150"
UI "SubText --Text=`"Choose your prefered theme`"" 
UI "Progress -Hide"

UI "Input --Type=ImageChooser --Header=`"Select theme`" --AllowedValues=`"$PSScriptRoot/WinDark.png#Dark theme|$PSScriptRoot/WinLight.jpg#Light theme`" --Width=300 --Button=Save --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE
$THEME = $(Get-Content -Path $INPUTFILE)
Remove-Item $INPUTFILE


### Use the following code to setup theme 

$THEME_FOLDER = "C:\Windows\resources\Themes"

## Launch the lines bellow in user context
If($Theme -eq "$PSScriptRoot/WinDark.png"){
    rundll32.exe themecpl.dll,OpenThemeAction "$THEME_FOLDER/dark.theme"
}Else{
    rundll32.exe themecpl.dll,OpenThemeAction "$THEME_FOLDER/aero.theme"
}

## Wait for process to start
$loop=0
While("$(Get-Process SystemSettings -ErrorAction SilentlyContinue)" -eq ""){
    Start-Sleep -Milliseconds  100
    $loop=+1
    If($loop -ge 10){
        Continue
    }
}
Start-Sleep -Seconds 1
## Close process
$loop=0
While("$(Get-Process SystemSettings -ErrorAction SilentlyContinue)" -ne ""){
    Start-Sleep -Milliseconds  100
    $loop=+1
    If($loop -ge 10){
        Continue
    }
    Get-Process SystemSettings -ErrorAction SilentlyContinue | Stop-Process -force
}
## Launch the lines above in user context


################################################################################
############################### Reboot #########################################
################################################################################
UI "MainText --Text=`"Finalization"
UI "MainImage --Source=`"$PSScriptRoot/restart.png`" --Height=150"
UI "SubText --Text=`"Your device is ready to go but needs a restart, please wait.`""
UI "Progress --Type=Indeterminate"

Start-Sleep -Seconds  3
UI "Terminate"