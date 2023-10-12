################################################################################
###################### Global functions and variables ##########################
################################################################################

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
Start-Process -FilePath "./UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$PSScriptRoot/$FILE`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"`" -AlwaysOnTop" -NoNewWindow | Out-Null



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


Start-Sleep -Seconds  1
UI "Terminate"