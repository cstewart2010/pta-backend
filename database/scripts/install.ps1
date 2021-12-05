# check environment
if (!$env:Database){
    Write-Warning "Database has not been set."
    Write-Host "Setting Database environment variable to PTA"
    [System.Environment]::SetEnvironmentVariable("Database", "PTA")
}

if (!$env:MongoDBConnectionString){
    Write-Warning "MongoDBConnectionString has not been set."
    Write-Host "Setting MongoDBConnectionString environment variable to mongodb://localhost:27017/$Database"
    [System.Environment]::SetEnvironmentVariable("MongoDBConnectionString", "mongodb://localhost:27017/$Database")
}

if (!$env:CookieKey){
    throw "Missing CookieKey Environment Variable"
}

Write-Host Running install.ps1 to install any missing tools
$isNodejsInstall = $null -ne (Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object { $_.DisplayName -ne $null -and $_.DisplayName -eq "Node.js" })
if (!$isNodejsInstall){
    Write-Host Installing Node.js
    Invoke-WebRequest -uri https://nodejs.org/dist/v16.13.0/node-v16.13.0-x64.msi -OutFile nodeinstall.msi
    Start-Process -FilePath "MsiExec.exe" -ArgumentList /i,nodeinstall.msi,/qn -NoNewWindow -Wait
    Remove-Item -Path nodeinstall.msi
    Write-Host Node.js installed
}

$isMongoShellInstall = $null -ne (Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object { $_.DisplayName -ne $null -and $_.DisplayName -eq "MongoDB Shell" })
if (!$isMongoShellInstall){
    Write-Host Installing MongoDB Shell
    Invoke-WebRequest -uri https://downloads.mongodb.com/compass/mongosh-1.1.2-x64.msi -OutFile mongoshellinstall.msi
    Start-Process -FilePath "MsiExec.exe" -ArgumentList /i,mongoshellinstall.msi,/qn,/norestart -NoNewWindow -Wait
    Remove-Item -Path mongoshellinstall.msi
    Write-Host MongoDB Shell installed
}

$isMongoDBInstall = $null -ne (Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\* | Where-Object { $_.DisplayName -ne $null -and $_.DisplayName.Contains("MongoDB 5") })
if (!$isMongoDBInstall){
    Write-Host Installing MongoDB
    Invoke-WebRequest -uri https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.4-signed.msi -OutFile mongodb.msi
    Start-Process -FilePath "MsiExec.exe" -ArgumentList /i,mongodb.msi,/qb -NoNewWindow -Wait
    Remove-Item -Path mongoshellinstall.msi
    Write-Host MongoDB Shell installed
}

Write-Host All tools installed
if ($env:MongoDBConnectionString){
    Write-Host Running mongo update script
    $CurrentDirectory = [System.IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path)
    $updateScript = Start-Process -FilePath "$env:LocalAppData\Programs\mongosh\mongosh.exe" -ArgumentList $env:MongoDBConnectionString,-f,"$CurrentDirectory\update.js" -NoNewWindow -PassThru -Wait
    if ($updateScript.ExitCode -ne 0){
        throw "Update script failed radically"
    }
}
else {
    throw "MongoDBConnectionString Environment Variable has not been set"
}

Write-Host Install script is complete
Read-Host