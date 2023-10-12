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


################################################################################
############################### UI Launch ######################################
################################################################################

# Launch app
Invoke-AsCurrentUser {
    Start-Process -FilePath "$($Argv[0])/UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$($Argv[1])`" --WindowTitle=`"BitLocker`" --WelcomeMessage=`"`" -AlwaysOnTop  --Width=670 --Height=690" -NoNewWindow | Out-Null
} -Arguments @(,$PSScriptRoot,$FILE)


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

    UI "Progress --Type=`"Determinate`" --Value=50 -ShowPercentage"
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
UI "Load --Text=`"Setting PIN...`""
Start-Sleep -Seconds 1
UI "Load -Hide"
UI "SubText --Text=`"BitLocker successfully setup.`"" 
Start-Sleep -Seconds 2
UI "Terminate"