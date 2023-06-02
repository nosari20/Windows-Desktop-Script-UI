Import-Module "$PSScriptRoot/modules/Invoke-AfterESP"

$ONBOARDING_DIRECTORY = "$env:ProgramFiles\Onboarding"

# Copy onboarding files
Remove-Item -Path "$ONBOARDING_DIRECTORY" -Force -Recurse
Copy-Item -Path "$PSScriptRoot\OnboardingUI" -Destination "$ONBOARDING_DIRECTORY" -Force -Recurse



# Launch onboarding after ESP
Invoke-AfterESP -ScriptBlock {
    
    Start-Process "powershell.exe" "-ExecutionPolicy Bypass -File `"$($Argv[0])\Onboarding.ps1`"" 

} -Arguments @($ONBOARDING_DIRECTORY) -TaskName "Onboarding"