$FILE = "demo.txt"

# Remove file if already exist
if (Test-Path $FILE) {
    Remove-Item $FILE | Out-Null
}

# Create file
New-Item $FILE | Out-Null

# Launch app
$process = Start-Process -FilePath "./UI/Windows Desktop Script UI.exe" -ArgumentList "--WatchPath=`"$PSScriptRoot/$FILE`" --WindowTitle=`"Hello World!`" --WelcomeMessage=`"Hello Folks`"" -NoNewWindow


# Demo Script
Start-Sleep -Seconds 3
"This is a simple demo" | out-file -append $FILE
Start-Sleep -Seconds 3
"for my project" | out-file -append $FILE
Start-Sleep -Seconds 3
"You can control what is display on this window" | out-file -append $FILE
Start-Sleep -Seconds 3
"from a PowerShell script" | out-file -append $FILE
Start-Sleep -Seconds 3
"by simply writing to a file." | out-file -append $FILE
Start-Sleep -Seconds 3
"Thank you!" | out-file -append $FILE
Start-Sleep -Seconds 3
"Terminate" | out-file -append $FILE