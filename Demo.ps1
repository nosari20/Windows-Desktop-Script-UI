$FILE = "demo.txt"

# Remove file if already exist
if (Test-Path $FILE) {
    Remove-Item $FILE | Out-Null
}

# Create file
New-Item $FILE | Out-Null

# Launch app
Start-Process -FilePath "./UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$PSScriptRoot/$FILE`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"`"" -NoNewWindow | Out-Null


# Demo Script
"MainText --Text='Hello $($env:UserName)'" | out-file -append $FILE
"MainImage --Source='$PSScriptRoot/Windows_logo.png' --Height=200" | out-file -append $FILE
"SubText --Text='We are setting up your device, please wait.'" | out-file -append $FILE
"Progress --Type='Determinate' --Value=40 -ShowPercentage" | out-file -append $FILE

for ($num = 1 ; $num -le 100 ; $num++)
{
    Start-Sleep -Milliseconds  200
    "Progress --Type='Determinate' --Value=$num -ShowPercentage" | out-file -append $FILE
}
"Progress --Type='Indeterminate" | out-file -append $FILE
"MainText --Text='Thank You!'" | out-file -append $FILE
"SubText --Text='Your device is ready to go but needs a restart, please wait.'" | out-file -append $FILE
