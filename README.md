PTA BackEnd 
---

### Table Of Contents
- [First Time User Setup](#first-time-user-setup)
  * [Environment Variables](#environment-variables)
  * [Installation](#installation)
  * [Build](#build)
  * [Run](#run)
- [Pokemon](#pokemon)
  * [Find Pokemon By Id](#find-pokemon-by-id)
  * [Trade Pokemon](#trade-pokemon)
  * [Update Pokemon Stats](#update-pokemon-stats)
  * [Delete Pokemon by Id](#delete-pokemon-by-id)
  * [Evolve Pokemon](#evolve-pokemon)
- [Trainer](#trainer)
  * [Verify Login](#verify-login)
  * [Verify Logout](#verify-logout)
  * [Find Trainer Pokemon](#find-trainer-pokemon)
  * [Add Trainer Pokemon](#add-trainer-pokemon)
  * [Add Items](#add-items)
  * [Remove Items](#remove-items)
  * [Delete Trainer](#delete-trainer)
- [Game](#game)
  * [Find Game by Id](#find-game-by-id)
  * [Find Trainer In Game](#find-trainer-in-game)
  * [Create New Game](#create-new-game)
  * [Add New Trainer to Game](#add-new-trainer-to-game)
  * [Create Wild Pokemon](#create-wild-pokemon)
  * [Import Game](#import-game)
  * [Start Game](#start-game)
  * [End Game](#end-game)
  * [Add Npcs to Game](#add-npcs-to-game)
  * [Remove Npcs from Game](#remove-npcs-from-game)
  * [Reset Trainer password](#reset-trainer-password)
  * [Delete Game](#delete-game)
  * [Export Game](#export-game)
- [Npc](#npc)
  * [Find Npc by Id](#find-npc-by-id)
  * [Create new Npc](#create-new-npc)
  * [Delete Npc](#delete-npc)
- [Consumption Only](#consumption-only)

---

# First Time User Setup

## Environment Variables
MongoUsername = **dbusername**  
MongoPassword = **dbPassword**  
Database = **db**  
MongoDBConnectionString = mongodb+srv://**$env:MongoUsername**:**$env:MongoPassword**@**url/to/mongo**/**$env:Database**?retryWrites=true&w=majority  
Example: mongodb+srv://**dbAdmin**:**dbPassword**@**pokemontabletopdatabase.com**/**PTA**?retryWrites=true&w=majority  
[Back to top](#table-of-contents)

---

## Installation

Run the install powershell script in the database/scripts directory to install any missing applications

```ps1
./database/scripts/install.ps1
Running install.ps1 to install any missing tools
All tools installed
Running mongo update script
```
[Back to top](#table-of-contents)

---

## Build

Build and test the solution before deployment

```cmd
dotnet build ./src/PTABackend.sln # builds the solutions
dotnet test ./src/PTABackend.sln # runs the entire unit test suite
dotnet test ./src/PTABackend.sln --filter Category=smoke # only runs the smoke test
```
[Back to top](#table-of-contents)

---

## Run

Not yet configured  
[Back to top](#table-of-contents)

---

# Pokemon
*Resource - api/v1/pokemon*  
[Back to top](#table-of-contents)
- [Find Pokemon By Id](#find-pokemon-by-id)
- [Trade Pokemon](#trade-pokemon)
- [Evolve Pokemon](#evolve-pokemon)
- [Delete Pokemon by Id](#delete-pokemon-by-id)

---

## Find Pokemon By Id
Endpoint - {pokemonId}  
Method - GET  
Response Type - PokemonModel  
[Back to Pokemon](#pokemon)

---

## Trade Pokemon
Endpoint - trade  
Method - PUT  
Response Type - { leftPokemon: PokemonModel, rightPokemon: PokemonModel }  
Requires a ptaSessionAuth cookie  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|
|leftPokemonId|string|A pokemonId|required|
|leftTrainerId|string|A trainerId|required|
|rightPokemonId|string|A pokemonId|required|
|rightTrainerId|string|A trainerId|required|

---

## Evolve Pokemon
Endpoint - evolve/{pokemonId}  
Method - PUT  
Response Type - PokemonModel  
Requires a ptaSessionAuth cookie  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerId|string|Trainer Id|required|
|nextForm|string|Evolution species namence|required|

---

## Delete Pokemon by Id
Endpoint - {pokemonId}  
Method - DELETE  
Response Type - message  
Requires a ptaSessionAuth cookie  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|

---

# Trainer
*Resource - api/v1/trainer*  
[Back to top](#table-of-contents)
- [Find Trainer Pokemon](#find-trainer-pokemon)
- [Verify Login](#verify-login)
- [Verify Logout](#verify-logout)
- [Add Trainer Pokemon](#add-trainer-pokemon)
- [Add Items](#add-items)
- [Remove Items](#remove-items)
- [Delete Trainer](#delete-trainer)

---

## Find Trainer Pokemon
Endpoint - {trainerId}/{pokemonId}  
Method - GET  
Response Type - PokemonModel  
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerId|string|Trainer Id|required|
|pokemonId|string|Pokemon Id|required|

---

## Add Trainer Pokemon
Endpoint - {trainerId}  
Method - POST  
Response Type - PokemonModel  
Requires a ptaSessionAuth cookie  
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|
|pokemon|string|A pokemon's name|required|
|nature|string|A pokemon's nature|required|
|gender|string|A pokemon's gender|required|
|status|string|A pokemon's status|required|
|nickname|string|The pokemon's nickname|optional|

---

## Verify Login
Endpoint - login  
Method - PUT  
Response Type - TrainerModel  
Grant a ptaSessionAuth cookie  
[Back to Trainer](#trainer)

---

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerName|string|Trainer's username|required|
|password|string|Trainer's password|required|
|gameId|string|The game id for the relevant game|required|

---

## Verify Logout
Endpoint - login  
Method - PUT  
Response Type - Status(200)  
Requires a ptaSessionAuth cookie  
[Back to Trainer](#trainer)

---

## Add Items
Endpoint - {trainerId}/addItems  
Method - PUT  
Response Type - PokemonModel  
Requires a ptaSessionAuth cookie  
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|
|{itemName}|int|>0|at least one item|

---

## Remove Items
Endpoint - {trainerId}/addItems  
Method - PUT  
Response Type - PokemonModel  
Requires a ptaSessionAuth cookie  
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|{itemName}|int|>0|at least one item|

---

## Delete Trainer
Endpoint - {trainerId}  
Method - DELETE  
Response Type - message  
Requires a ptaSessionAuth cookie  
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|

---

# Game
*Resource - api/v1/game*  
[Back to top](#table-of-contents)
- [Find Game by Id](#find-game-by-id)
- [Find Trainer In Game](#find-trainer-in-game)
- [Create New Game](#create-new-game)
- [Add New Trainer to Game](#add-new-trainer-to-game)
- [Create Wild Pokemon](#create-wild-pokemon)
- [Import Game](#import-game)
- [Start Game](#start-game)
- [End Game](#end-game)
- [Add Npcs to Game](#add-npcs-to-game)
- [Remove Npcs from Game](#remove-npcs-from-game)
- [Reset Trainer password](#reset-trainer-password)
- [Delete Game](#delete-game)
- [Export Game](#export-game)

## Find Game by Id
Endpoint - {gameId}  
Method - GET  
Response Type - GameModel  
[Back to Game](#game)

---

## Find Trainer In Game
Endpoint - {gameId}/find/{trainerId}  
Method - GET  
Response Type - GameModel  
[Back to Game](#game)

---

## Create New Game
Endpoint - new  
Method - POST  
Response Type - GameModel  
Grants a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|nickname|string|a nickname for the game session|optional|
|gmUsername|string|GM's username|required|
|gmPassword|string|GM's password|required|
|gameSessionPassword|string|the password for the game session|required|

---

## Add New Trainer to Game
Endpoint - {gameId}/new  
Method - PUT  
Response Type - TrainerModel  
Grants a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerName|string|Trainer's username|required|
|password|string|Trainer's password|required|
|attack|int|The trainer's attack stat|required, between 1 and 10|
|defense|int|The trainer's defense stat|required, between 1 and 10|
|specialAttack|int|The trainer's special attack stat|required, between 1 and 10|
|specialDefense|int|The trainer's special defense stat|required, between 1 and 10|
|speed|int|The trainer's speed stat|required, between 1 and 10|

---

## Import Game
Endpoint - import  
Method - POST  
Content Type - application/octet-stream  
Response Type - Ok  
Expects a .json file matching the ExportedGame schema

---

## Create Wild Pokemon
Endpoint - {gameMasterId}/wild  
Method - POST  
Response Type - PokemonModel  
Requires a ptaSessionAuth cookie  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|pokemon|string|A pokemon's name|required|
|nature|string|A pokemon's nature|required|
|gender|string|A pokemon's gender|required|
|status|string|A pokemon's status|required|
|nickname|string|The pokemon's nickname|optional|

---

## Start Game
Endpoint - {gameId}/start  
Method - PUT  
Response Type - Status(200)  
Grants a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameSessionPassword|string|Game's password|required
|gmUsername|string|Trainer's username|required|
|gmPassword|string|Trainer's password|required|

---

## End Game
Endpoint - {gameId}/end  
Method - PUT  
Response Type - Status(200)  
Requires a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|

---

## Add Npcs to Game
Endpoint - {gameId}/addNpcs  
Method - PUT  
Response Type - Status(200)  
Requires a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|
|npcIds|string[]|npcs to add to the game|required|

---

## Remove Npcs from Game
Endpoint - {gameId}/removeNpcs  
Method - PUT  
Response Type - Status(200)  
Requires a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|
|npcIds|string[]|npcs to remove from the game|required|

---

## Reset Trainer password
Endpoint - reset  
Method - PUT  
Response Type - TrainerModel  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerId|string|Trainer's id|required|
|password|string|Trainer's password|required|

---

## Delete Game
Endpoint - {gameId}  
Method - DELETE  
Response Type - GameModel  
Requires a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|
|gameSessionPassword|string|Game's password|required|

---

## Export Game
Endpoint -{gameId}/export  
Method - Delete  
Response Type - Ok  
Requires a ptaSessionAuth cookie  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameMasterId|string|GameMaster's id|required|
|gameSessionPassword|string|Game's password|required|

---

# Npc
*Resource - api/v1/npc*  
[Back to top](#table-of-contents)
- [Find Npc by Id](#find-npc-by-id)
- [Create new Npc](#create-new-npc)
- [Delete Npc](#delete-npc)

## Find Npc by Id
Endpoint - {npcId}  
Method - GET  
Response Type - NpcModel  
[Back to Npc](#npc)

---

## Create New Npc
Endpoint - new  
Method - POST  
Response Type - NpcModel  
[Back to Npc](#npc)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerName|string|Trainer's username|required|
|feats|string[]|comma separated list of feats|optional|
|classes|string[]|comma separated list of trainer classes|optional|

---

## Delete Npc
Endpoint - {npcId}  
Method - DELETE  
Response Type - Status(200)  
[Back to Npc](#npc)

---

# Consumption Only
[Back to top](#table-of-contents)
- [Pokedex](#pokedex)
- [Berries](#berries)
- [Features](#features)
- [Items](#items)
- [Moves](#moves)
- [Origins](#origins)
- [Trainer Classes](#trainer-classes)

---

## Pokedex
*Resource - api/v1/pokedex*  
Endpoints
- {name}

Response Type - BasePokemonModel  
[Back to Consumption Only](#consumption-only)

---

## Berries
*Resource - api/v1/berrydex*  
Endpoints
- {name}

Response Type - BerryModel  
[Back to Consumption Only](#consumption-only)

---

## Features
*Resource - api/v1/featuredex*  
Endpoints
- general/{name}
- legendary/{name}
- passives/{name}
- skills/{name}

Response Type - FeatureModel  
[Back to Consumption Only](#consumption-only)

---

## Items
*Resource - api/v1/itemdex*  
Endpoints
- key/{name}
- medical/{name}
- pokeball/{name}
- pokemon/{name}
- trainer/{name}

Response Type - BaseItemModel  
[Back to Consumption Only](#consumption-only)

---

## Moves
*Resource - api/v1/movedex*  
Endpoints
- {name}

Response Type - MoveModel  
[Back to Consumption Only](#consumption-only)

---

## Origins
*Resource - api/v1/origindex*  
Endpoints
- {name}

Response Type - OriginModel  
[Back to Consumption Only](#consumption-only)

---

## Trainer Classes
*Resource - api/v1/classdex*  
Endpoints
- {name}

Response Type - TrainerClassModel  
[Back to Consumption Only](#consumption-only)