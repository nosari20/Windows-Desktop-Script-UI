
################################################################################
############################# Run after logon ##################################
################################################################################
Function Invoke-AfterESP(){

    param(
        [Parameter(Mandatory=$true, Position=0)] 
        [ValidateNotNullOrEmpty()]
        [Scriptblock]$ScriptBlock,

        [Parameter(Position=1)] 
        [string[]]$Arguments,

        [Parameter(Position=2)] 
        [string]$TaskName = "Invoke-AfterESP-$(Get-Random -Maximum 1000)"
    )

    # Create arguments
    $Argv ="`$Argv = @("
    If($Arguments){
        ForEach($Argument in $Arguments){
            $Type = "String"
            $Argument
            Try{
                $Type = ([int]$Argument).GetType().Name
            }Catch{}
            $Type
            If($Type -eq 'Int32'){
                $Argv += ",$Argument"
            }Else{
                $Argv += ",'$Argument'"
            }      
        }
    }
    $Argv += ")`n"

    # Put code in temp file
    ($Argv + $ScriptBlock.ToString()) | Out-File -FilePath "C:\Users\Public\$taskName.ps1"
    "Remove-Item -Path 'C:\Users\Public\$taskName.ps1';Unregister-ScheduledTask -TaskName '$taskName' -Confirm:`$false" | Out-File -FilePath "C:\Users\Public\$taskName.ps1" -Append

    # Define action
    $action = New-ScheduledTaskAction -Execute "powershell.exe" "-WindowStyle Hidden -ExecutionPolicy Bypass -File C:\Users\Public\$taskName.ps1"

    # Define trigger to run the task after logon
    $trigger = New-ScheduledTaskTrigger -Once -At ((Get-date).AddSeconds(10))

    # Define targeted user
    $principal = New-ScheduledTaskPrincipal -User "NT AUTHORITY\SYSTEM" -LogonType ServiceAccount
    
    # Allow start if on battery
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries

    # Create task
    $task = New-ScheduledTask -Action $action -Trigger $trigger -Principal $principal -Settings $settings
    
    # Register task
    Register-ScheduledTask $taskName -InputObject $task | Out-Null

    # Start Task
    #Start-ScheduledTask -TaskName $taskName
    
}

Export-ModuleMember -Function Invoke-AfterESP
