PTA BackEnd (restarting)
---

## Enivronment
### .NET
The Pokemon Tabletop Adventures Web API run on [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), to be upgraded to [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

### NodeJs
[NodeJs](https://nodejs.org/en/download) is required to install the Allure Test Reporting Framework

### Java
[Java](https://www.oracle.com/java/technologies/javase/jdk24-archive-downloads.html) is required to generate or serve an Allure Report

### Environment Variables
THe following environment variables are necessary for the application
```
CookieKey={insert-value}
MongoDBConnectionString={mongo-connection-string}
Database={name-of-mongo-database}
```

## Applications 
### MongoDB
Install [MongoDB](https://www.mongodb.com/) Atlas and mongosh to start initializing the local database

### PTA Tools
Use the MongoDbImportTool from [pta-tools](https://github.com/cstewart2010/pta-tools) to initialize a local database

### Allure
Allure is used to visualize the integration test report
```sh
npm install -g allure
```

## Running the application
### Build
```sh
dotnet build
```

### Test
```sh
dotnet test test/PokemonTabletopAdventures.CoreApi.UnitTests
```

### Run
```sh
dotnet run --project src/PokemonTabletopAdventures.CoreApi/PokemonTabletopAdventures.CoreApi.csproj
```

### Run Integration Tests
```sh
dotnet test test/PokemonTabletopAdventures.CoreApi.IntegrationTests
```

### Run Allure
```sh
allure generate
allure serve
```