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

$global:FileChanged = $false
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




# Launch app
Start-Process -FilePath "./UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$PSScriptRoot/$FILE`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"`" -FullScreen -Debug" -NoNewWindow | Out-Null
# Demo Script
UI "MainText --Text='Hello $($env:UserName)'"
UI "MainImage --Source='$PSScriptRoot/Windows_logo.png' --Height=200"
UI "SubText --Text='We are setting up BitLocker to protect your data.'"

$PIN = $False;
$PINRE = $True;

while ($PIN -ne $PINRE) {

    UI "Progress --Type='Determinate' --Value=33 -ShowPercentage"

    $INPUTFILE = "./out"
    UI "Input --Type=Password --PlaceHolder='Florent NOSARI' --Header='Type BitLocker password' --Button=OK --Out=$INPUTFILE"
    Wait-FileChange -File $INPUTFILE
    $PIN = $(Get-Content -Path $INPUTFILE)
    Remove-Item $INPUTFILE

    UI "Progress --Type='Determinate' --Value=66 -ShowPercentage"
    Start-Sleep -Seconds 1

    UI "Input --Type=Password --PlaceHolder='Florent NOSARI' --Header='Retype BitLocker password' --Button=OK --Out=$INPUTFILE"
    Wait-FileChange -File $INPUTFILE
    $PINRE = $(Get-Content -Path $INPUTFILE)
    Remove-Item $INPUTFILE
    
}



UI "Progress --Type='Determinate' --Value=100 -ShowPercentage"
UI "SubText --Text='BitLocker successfully setup.'" 

Start-Sleep -Seconds 2

UI "Progress --Type='Indeterminate"
UI "MainText --Text='Thank You!'"
UI "SubText --Text='Your device is ready to go but needs a restart, please wait.'"

Start-Sleep -Seconds  3
UI "Terminate"