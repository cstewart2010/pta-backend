Write-Host Running .\testbuild.ps1
if (!$env:MongoDBConnectionString){
    throw "Missing MongoDBConnectionString"
}

if (!$env:MongoUsername){
    throw "Missing MongoDBConnectionString"
}

if (!$env:MongoPassword){
    throw "Missing MongoDBConnectionString"
}
dotnet build src/PTABackend.sln
mongosh $env:MongoDBConnectionString -f database\scripts\update.js
mongosh "mongodb+srv://$env:MongoUsername:$env:MongoPassword@ptatestcluster.1ekcs.mongodb.net/test?retryWrites=true&w=majority" -f database\scripts\update.js
$env:MongoDBConnectionString = "mongodb+srv://$env:MongoUsername:$env:MongoPassword@ptatestcluster.1ekcs.mongodb.net/test?retryWrites=true&w=majority"
dotnet test src/PTABackend.sln --logger:trx;LogFileName=TestOutput.trx --filter:Category=smoke