# Windows Desktop Script UI

[![License MIT](https://img.shields.io/badge/license-MIT-green)](https://opensource.org/license/mit/)
[![Version beta](https://img.shields.io/badge/version-rc2-green)](https://github.com/nosari20/Windows-Desktop-Script-UI/releases)

![Visual Studio](https://img.shields.io/badge/Visual%20Studio-5C2D91.svg?style=for-the-badge&logo=visual-studio&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Windows App SDK](https://img.shields.io/badge/Windows%20App%20SDK-0078D6?style=for-the-badge&logo=windows&logoColor=white)

![Screenshot](/Screenshot.png)

[Full demo video here](/Demo.mp4)

Latest release and demo code available [here](https://github.com/nosari20/Windows-Desktop-Script-UI/releases)


# Presentation

This project aims to offer easy to use and modern UI for Windows desktop devices during setup. You can use it from PowerShell scripts by writing commands in a file wich is watched by the app.

Samples:
* [Onboarding screen](https://github.com/nosari20/Windows-Desktop-Script-UI/tree/master/samples/Onboarding)
* [Bitlocker PIN setup](https://github.com/nosari20/Windows-Desktop-Script-UI/tree/master/samples/BitLockerPIN)

## Tech

This project is developped with the following libraries:
* Windows App SDK
* WinUI 3
* .NET 8


## Prerequisites

Target devices must have the following runtime installed:
* [Windows App SDK 1.5](https://learn.microsoft.com/fr-fr/windows/apps/windows-app-sdk/downloads#windows-app-sdk-15)
* [.NET 8](https://dotnet.microsoft.com/en-us/download) (not required for self-contained build)

## Usage

### Launch UI

```ps1
# Launch app
Start-Process -FilePath "./Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"commands.txt`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"Hello Folks`"" -NoNewWindow | Out-Null
```
* `--WatchPath`      : path of command file
* `--WindowTitle`    : title of widow (optional)
* `--WelcomeMessage` : main text (optional)
* `--Height`         : window height (optional)
* `--Width`          : window width (optional)
* `-FullScreen`      : fullscreen flag (optional)
* `-AlwaysOnTop`     : always on top flag (optional)
* `--Opacity`        : window opacity (optional)
* `-Debug`           : debug flag (optional)


Note: if you are running in system context you must launch it in user context (with [KelvinTegelaar/RunAsUser](https://github.com/KelvinTegelaar/RunAsUser) or [Microsoft deployment toolkit](https://www.microsoft.com/en-us/download/details.aspx?id=54259) ServiceUI.exe for example)


### Manipulate UI

The app listen to commands written in a file, you just have to write your own function to write to the command file.

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
UI "Terminate"
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
UI "Load --Text='Please wait..'"
```
* `--Text`           : text behind progresss ring (optional, use \n for line break)
* `-Hide`            : hide progress ring (optional)

#### Input
```ps1
   UI "Input --Type=Password --PlaceHolder='Florent NOSARI' --Header='Type BitLocker password' --Button=OK --Out=input.txt"
```
* `--Type`           : input type (see details below)
* `--PlaceHolder`    : placeholder for supported types (optional)
* `--Value`          : default value for supported types(optional)
* `--AllwowedValues` : allowed values for supported types (optional)
* `--Header`         : header for supported types (optional)
* `--Button`         : submit button text (optional)
* `--Out`            : file path where user input is store after submit (optional)
* `--Height`         : input height (optional)
* `--Width`          : input width (optional)


User input is stored in a file (file is empty if value is not provided on input is simple button), you have to wait for file change before continuig you script, here is a short example.

```ps1
## Function towait for file change
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

| Type          | Description                          | WINUI 3 Object      | Supported options                                                                 |
|---------------|--------------------------------------|---------------------|-----------------------------------------------------------------------------------|
| `Text`        | Basic text field                     | `TextBox`           | Header<br>PlaceHolder<br>Value                                                    |
| `Password`    | Password text field                  | `PasswordBox`       | Header<br>                                                                        |
| `ComboBox`    | Predefined value selector            | `ComboBox`          | Header<br>AllowedValues (separated by "\|")<br>Value (represent the default value)|
| `ImageChooser`| Grid with images as available values | `GridView`          | Header<br>AllowedValues (separated by "\|")                                       |
| `ButtonImage` | Button with image to display         | `Image`             |                                                                                   |
| `ButtonVideo` | Button with video                    | `MediaPlayerElement`| Autoplay<br>ShowControl<br>SoundOn                                                |
| `ButtonText`  | Button with rich text                | `RichTextBlock`     |                                                                                   |


```ps1
# Text
UI "Input --Type=Text --Header='Enter value' --Button=OK --Out=$INPUTFILE"

# Password
UI "Input --Type=Password --Header='Enter password' --Button=OK --Out=$INPUTFILE"

# ComboBox
UI "Input --Type=ComboBox --Header='Select option' --AllowedValues='One|Two|Three' --Value='Two' --Button=OK --Out=$INPUTFILE"

# ImageChooser
UI "Input --Type=ImageChooser --Header='Select option' --AllowedValues='$PSScriptRoot/One.png|$PSScriptRoot/Two.png|$PSScriptRoot/Three.png' --Button=OK --Out=$INPUTFILE"

# ButtonImage
UI "Input --Type=ButtonImage --Value='$PSScriptRoot/Image.png' --Button='Next' --Out=$INPUTFILE"

# ButtonVideo
UI "Input --Type=ButtonImage --Value='$PSScriptRoot/Video.mp4' --Button='Next' --Out=$INPUTFILE"

# ButtonText
$content="<Paragraph>Lorem ipsum dolor sit amet.</Paragraph>" # be aware of " and ' interpretation
UI "Input --Type=ButtonImage --Value='$content' --Button='Accept' --Out=$INPUTFILE"

```

ButtonText value must be valid XAML wich can be included in `RichTextBlock` ([See documentation](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.richtextblock?view=windows-app-sdk-1.3)). Find an example below:

```xml
<Paragraph TextAlignment="Center">
    <InlineUIContainer>
        <Image 
            Source="C:\temp\contoso.png"
            Width="100"/>
    </InlineUIContainer>
</Paragraph>
<Paragraph>
    Lorem ipsum dolor sit amet, consectetur
    adipiscing elit. Duis sit amet nisi eget ex gravida molestie. Nulla varius leo at nulla
    molestie, sit amet ultricies nisi efficitur. Proin non massa eros. Fusce convallis maximus risus
    ac aliquam. Fusce non tempus orci, at dignissim velit. Nulla at sollicitudin arcu. Proin arcu
    mi, gravida at suscipit a, gravida eu lorem. Nulla commodo, mi sit amet tincidunt pharetra,
    justo ipsum malesuada mi, eget malesuada est elit at ipsum.
</Paragraph>
<Paragraph>
    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis sit amet nisi eget ex
    gravida molestie. Nulla varius leo at nulla molestie, sit amet ultricies nisi efficitur. Proin
    non massa eros. Fusce convallis maximus risus ac aliquam. Fusce non tempus orci, at dignissim
    velit. Nulla at sollicitudin arcu. Proin arcu mi, gravida at suscipit a, gravida eu lorem. Nulla
    commodo, mi sit amet tincidunt pharetra, justo ipsum malesuada mi, eget malesuada est elit at
    ipsum.
</Paragraph>
```

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
UI "SubText --Text=`"Welcome to Windows, let's setup your device.`""
UI "Input --Type=ButtonVideo --Value=`"$PSScriptRoot/Windows11.mp4`" --Button=`"Continue`" --Height=300 --Width=500 -Autoplay --Out=`"$INPUTFILE`""
Wait-FileChange -File $INPUTFILE

################################################################################
############################### Reboot #########################################
################################################################################
UI "MainText --Text=`"Finalization"
UI "MainImage --Source=`"$PSScriptRoot/restart.png`" --Height=150"
UI "SubText --Text=`"Your device is ready to go but needs a restart, please wait.`""
UI "Load --Type='Waiting for reboot...'"

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
- [x] ProgressBar hide
- [ ] Info Bar
- [x] Input text
- [x] Input password
- [x] Input button (use submit button)
- [x] Input combobox
- [x] Input image chooser
- [x] Input text
- [x] Input image
- [x] Input video
- [ ] Input toggle switch
- [ ] Input FlipView
- [ ] Input WebView
- [ ] Input FilePicker

## Scenarios ideas

- BitLocker PIN
- Dark/light mode chooser
- Prefered wallpaper
- Theme color
- Application installation (if user input is needed)


## Contribute

The following tools are required to edit:
* Visual Studio with Windows App SDK components ([instructions](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b))

Build single file
```powershell
dotnet publish -c Release -p:Platform=x64 -p:PublishSingleFile=true --self-contained=true -p:WindowsAppSDKSelfContained=true
```

## Inspiration

This project was inspired by [Mactroll/DEPNotify](https://gitlab.com/Mactroll/DEPNotify) (macOS)
