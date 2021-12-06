Function Invoke-MsiExecInstall {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Uri,
        [Parameter(Mandatory=$true)]
        [string[]]$InstallArguments
    )
    
    $filename = "$([System.Guid]::NewGuid()).msi"
    Invoke-WebRequest -uri $Uri -OutFile  $filename
    Start-Process -FilePath "MsiExec.exe" -ArgumentList "/i $filename $InstallArguments" -NoNewWindow -Wait
    Remove-Item -Path $filename
}

Function Test-UserRights {
    $identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object System.Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
}

Function Update-EnvironmentVariable {
    param (
        [Parameter(Mandatory=$true)]
        [string]$EnvironmentVariable,
        [Parameter(Mandatory=$true)]
        [string]$Value
    )
    
    Write-Host "Setting $EnvironmentVariable environment variable to $Value"
    [System.Environment]::SetEnvironmentVariable($EnvironmentVariable, $Value, "Machine")
}

Function Update-Hashkey {
    Write-Host "Updating Environment Variables"
    if (![System.Environment]::GetEnvironmentVariable("CookieKey", "Machine")){
        Write-Warning "Missing Hashkey Environment Variable"
        while (!$key){
            $key = Read-Host -Prompt "Enter a Hashkey"
        }
    
        Update-EnvironmentVariable -EnvironmentVariable "CookieKey" -Value $key
    }
}

########################## Main ##########################
if (!(Test-UserRights)){
    throw "Please run this script with elevated admin rights"
}

# check environment
$Database = "PTA"
$MongoDBConnectionString = "mongodb://localhost:27017/$Database"

Update-Hashkey
Update-EnvironmentVariable -EnvironmentVariable "Database" -Value $Database
Update-EnvironmentVariable -EnvironmentVariable "MongoDBConnectionString" -Value $MongoDBConnectionString

# installing missing applications
Write-Host Running install.ps1 to install any missing tools
$isNodejsInstall = $null -ne (Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object { $_.DisplayName -ne $null -and $_.DisplayName -eq "Node.js" })
if (!$isNodejsInstall){
    Write-Host Installing Node.js
    Invoke-MsiExecInstall -Uri https://nodejs.org/dist/v16.13.0/node-v16.13.0-x64.msi -InstallArguments /qn
    Write-Host Node.js installed
}

$isMongoShellInstall = $null -ne (Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object { $_.DisplayName -ne $null -and $_.DisplayName -eq "MongoDB Shell" })
if (!$isMongoShellInstall){
    Write-Host Installing MongoDB Shell
    Invoke-MsiExecInstall -Uri https://downloads.mongodb.com/compass/mongosh-1.1.2-x64.msi -InstallArguments "/qn /norestart"
    Write-Host MongoDB Shell installed
}

$isMongoDBInstall = $null -ne (Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object { $_.DisplayName -ne $null -and $_.DisplayName.Contains("MongoDB 5") })
if (!$isMongoDBInstall){
    Write-Host Installing MongoDB
    Invoke-MsiExecInstall -Uri https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.4-signed.msi -InstallArguments /qb
    Write-Host MongoDB Shell installed
}

Write-Host All tools installed

# updating mongodb schemas
Write-Host Running mongo update script
$CurrentDirectory = [System.IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path)
$updateScript = Start-Process -FilePath "$env:LocalAppData\Programs\mongosh\mongosh.exe" -ArgumentList $MongoDBConnectionString,-f,"$CurrentDirectory\update.js" -NoNewWindow -PassThru -Wait
if ($updateScript.ExitCode -ne 0){
    throw "Update script failed radically"
}

Write-Host Install script is complete
Write-Host Run the MongoDBImportTool to finish adding missing dex items
Read-Host