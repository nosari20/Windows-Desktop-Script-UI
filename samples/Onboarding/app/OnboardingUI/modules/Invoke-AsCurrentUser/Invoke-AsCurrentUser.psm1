################################################################################
############################# Run as user ######################################
################################################################################

Function Invoke-AsCurrentUser(){

    param(
        [Parameter(Mandatory=$true, Position=0)] 
        [ValidateNotNullOrEmpty()]
        [Scriptblock]$ScriptBlock,

        [Parameter(Position=1)] 
        [string[]]$Arguments
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

    # Create random name for task
    $taskName="Invoke-AsCurrentUser-$(Get-Random -Maximum 1000)"

    # Put code in temp file
    ($Argv + $ScriptBlock.ToString()) | Out-File -FilePath "C:\Users\Public\$taskName.ps1"
    "Remove-Item -Path 'C:\Users\Public\$taskName.ps1'" | Out-File -FilePath "C:\Users\Public\$taskName.ps1" -Append

    # Define action
    $action = New-ScheduledTaskAction -Execute "powershell.exe" "-WindowStyle Hidden -ExecutionPolicy Bypass -File C:\Users\Public\$taskName.ps1"

    # Define trigger
    $trigger = New-ScheduledTaskTrigger -AtLogon

    # Define targeted user
    $User = Get-WmiObject Win32_Process -Filter "Name='explorer.exe'" | ForEach-Object { $_.GetOwner() } | Select-Object -Unique
    $principal = New-ScheduledTaskPrincipal -UserId "$($User.Domain)\$($User.User)"
    
    # Define settings
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries

    # Create task
    $task = New-ScheduledTask -Action $action -Trigger $trigger -Principal $principal -Settings $settings
    
    # Register task
    Register-ScheduledTask $taskName -InputObject $task | Out-Null

    # Start Task
    Start-ScheduledTask -TaskName $taskName

    # Unregister Task
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}
Export-ModuleMember -Function Invoke-AsCurrentUser