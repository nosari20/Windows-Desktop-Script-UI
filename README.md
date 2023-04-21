# Windows Desktop Script UI

![Alt text](/Screenshot.png?raw=true "Title")


# Presentation

This project aims to offer easy to use and modern UI for Windows desktop devices during setup. You can use it from PowerShell scripts by writing commands in a file wich is watched by the app.


## Tech

This project is developped with the following libraries:
* Windows App SDK
* WinUI 3


## Prerequisites

Target devices must have the following runtime installed:
* [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
* [.NET 6](https://dotnet.microsoft.com/en-us/download)

## Usage

### Launch UI

```ps1
# Launch app
Start-Process -FilePath "./Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"commands.txt`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"Hello Folks`"" -NoNewWindow | Out-Null
```
* `--WatchPath`      : path of command file
* `--WindowTitle`    : title of widow (optional)
* `--WelcomeMessage` : main text (optional)
* `-FullScreen`      : fullscreen flag (optional)
* `-Debug`           : debug flag (optional)


Note: if you are running in system context you must launche it in user context (with [KelvinTegelaar/RunAsUser](https://github.com/KelvinTegelaar/RunAsUser) or [Microsoft deployment toolkit](https://www.microsoft.com/en-us/download/details.aspx?id=54259) ServiceUI.exe for example)


### Manipulate UI

The app listen to command written in file, you just have to write your own system to write to file command file.

```ps1
# Define file path
$FILE = "demo.txt"

# Remove file if already exist
if (Test-Path $FILE) {
    Remove-Item $FILE | Out-Null
}

# Create file
New-Item $FILE | Out-Null

## Create writting function for clearer code
function UI {
    param(
        [Parameter(Position=0)]
        [string] $command
    )

    Write-Host($command)
    $command | out-file -append $FILE
}
```
#### Close window 
```ps1
UI "Terminate
```

#### Main text 
```ps1
UI "MainText --Text='Hello John'"
```
* `--Text` : text to display

#### Sub text 
```ps1
UI "SubText --Text='We are setting up BitLocker to protect your data.'"
```
* `--Text` : text to display

#### Main image 
```ps1
UI "MainImage --Source='$PSScriptRoot/Windows_logo.png' --Height=200 -Width=200"
```
* `--Source` : image source
* `--Height` : image height (optional)
* `--Width`  : image width (optional)

#### Progress
```ps1
UI "Progress --Type='Determinate' --Value=33 -ShowPercentage"
```
* `--Type`           : image source (Determinate or Indeterminate)
* `--Value`          : value between 0 and 100 (required for Determinate mode)
* `-ShowPercentage`  : show percentage under progressbar (optional)

#### Input
```ps1
   UI "Input --Type=Password --PlaceHolder='Florent NOSARI' --Header='Type BitLocker password' --Button=OK --Out=input.txt"
```
* `--Type`        : input type (see details below)
* `--PlaceHolder` : placeholder for supported type (optional)
* `--Header`      : header (optional)
* `--Button`      : submit button text (optional)
* `--Out`         : file path where user input is store after submit (optional)


User input is stored in a file, you have to wait for file change before continuig you script, here is a short example.

```ps1
## Function towait for file change
function Wait-FileChange {
    param(
        [string]$File,
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

....

# Define file path
$INPUTFILE = "./out"

# Request input
UI "Input --Type=Password --Header='Type BitLocker password' --Button=OK --Out=$INPUTFILE"

# Wait for file to be created
Wait-FileChange -File $INPUTFILE

## Get content
$PIN = $(Get-Content -Path $INPUTFILE)

# Remove file
Remove-Item $INPUTFILE
```

The following input types are supported:

| Type          | Description         | WINUI 3 Object     | Supported options |
|---------------|---------------------|--------------------|-------------------|
| `Text`        | Basic text field    | `TextBox`          | PlaceHolder       |
| `Password`    | Password text field | `PasswordBox`      |                   |


## Sample code

```ps1
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
```

## Todo

- [x] Window name
- [ ] Window icon
- [ ] User picture
- [x] Main text
- [x] Sub text 
- [x] Sub image 
- [x] ProgressBar inderterminate
- [x] ProgressBar derterminate
- [x] ProgressBar derterminate percentage
- [ ] Info Bar
- [x] Input text
- [x] Input password
- [ ] Input button (use submit button)
- [ ] Input combobox
- [ ] Input toggle switch
- [ ] Input FlipView

## Scenarios ideas

- BitLocker PIN
- Dark/light mode chooser
- Prefered wallpaper
- Theme color
- Appliction installation (if user input is needed)


## Contribute

The following tool are required to edit:
* Visual Studio with Windows App SDK components ([instructions](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b))

## Inspiration

This project was inspired by [Mactroll/DEPNotify](https://gitlab.com/Mactroll/DEPNotify) (macOS)