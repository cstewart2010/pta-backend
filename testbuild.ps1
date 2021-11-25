# check environment
if (!$env:MongoDBConnectionString){
    throw "Missing MongoDBConnectionString Environment Variable"
}

if (!$env:MongoUsername){
    throw "Missing MongoUsername Environment Variable"
}

if (!$env:MongoPassword){
    throw "Missing MongoPassword Environment Variable"
}

if (!$env:CookieKey){
    throw "Missing CookieKey Environment Variable"
}

# prebuild
Write-Host Running .\testbuild.ps1
$currentDirectory = (Get-Location).Path
$repository = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
Set-Location -Path $repository

# build
mongosh $env:MongoDBConnectionString -f database\scripts\update.js
dotnet build src/PTABackend.sln
$proc = Start-Process -FilePath ".\src\MongoDbImportTool\bin\Debug\netcoreapp3.1\MongoDbImportTool.exe" -NoNewWindow -PassThru -Wait
if ($proc.ExitCode -ne 0){
    return 1
}

# test
$env:Database = "test"
mongosh $env:MongoDBConnectionString -f  .\database\scripts\update.js
Write-Host "Updating the Test Environment"
$proc = Start-Process -FilePath ".\src\MongoDbImportTool\bin\Debug\netcoreapp3.1\MongoDbImportTool.exe" -PassThru -Wait
if ($proc.ExitCode -ne 0){
    return 1
}
dotnet test .\src\PTABackEnd.sln --logger:"trx;LogFileName=C:\Users\zachagrey\.jenkins\workspace\PTA backend develop build/TestOutput.trx" --filter:Category=smoke

# postbuild
$env:MongoDBConnectionString = "mongodb+srv://$($env:MongoUsername):$($env:MongoPassword)@ptatestcluster.1ekcs.mongodb.net/test?retryWrites=true&w=majority"
mongosh $env:MongoDBConnectionString -f .\database\scripts\catalog_logs.js
Set-Location -Path $currentDirectory