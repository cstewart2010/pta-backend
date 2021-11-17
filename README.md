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
  * [Create Wild Pokemon](#create-wild-pokemon)
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

---

# First Time User Setup

## Environment Variables
MongoDBConnectionString = mongodb+srv://**dbusername**:**dbPassword**@**url/to/mongo**/PTA?retryWrites=true&w=majority  
Example: mongodb+srv://**dbAdmin**:**dbPassword**@**pokemontabletopdatabase.com**/PTA?retryWrites=true&w=majority  
[Back to top](#Table-Of-Contents)

---

## Installation

Run the install powershell script in the database/scripts directory to install any missing applications

```ps1
./database/scripts/install.ps1
Running install.ps1 to install any missing tools
All tools installed
Running mongo update script
```
[Back to top](#Table-Of-Contents)

---

## Build

Build and test the solution before deployment

```cmd
dotnet build .src/PTABackend.sln # builds the solutions
dotnet test .src/PTABackend.sln # runs the entire unit test suite
dotnet test .src/PTABackend.sln --filter Category=smoke # only runs the smoke test
```
[Back to top](#Table-Of-Contents)

---

## Run

Not yet configured  
[Back to top](#Table-Of-Contents)

---

# Pokemon
*Resource - api/v1/pokemon*  
[Back to top](#Table-Of-Contents)
- [Find Pokemon By Id](#find-pokemon-by-id)
- [Create Wild Pokemon](#create-wild-pokemon)
- [Trade Pokemon](#trade-pokemon)
- [Update Pokemon Stats](#update-pokemon-stats)
- [Delete Pokemon by Id](#delete-pokemon-by-id)
- [Evolve Pokemon](#evolve-pokemon)

---

## Find Pokemon By Id
Endpoint - {pokemonId}  
Method - GET  
Response Type - PokemonModel  
[Back to Pokemon](#pokemon)

---

## Create Wild Pokemon
Endpoint - wild  
Method - POST  
Response Type - PokemonModel  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|pokemon|string|A pokemon's name|required|
|nature|string|A pokemon's nature|required|
|naturalMoves|list|A list of Pokemon moves|At least one, no more than four|
|expYield|int|Experience yield|>0|
|catchRate|int|A pokemon's catch rate|>=0|
|experience|int|Total experience|>=0|
|level|int|Current level|>0|
|tmMoves|list|A list of Pokemon moves|optional, no more than four|
|nickname|string|The pokemon's nickname|optional|

---

## Trade Pokemon
Endpoint - trade  
Method - PUT  
Response Type - { leftPokemon: PokemonModel, rightPokemon: PokemonModel }  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|leftPokemonId|string|A pokemonId|required|
|leftTrainerId|string|A trainerId|required|
|rightPokemonId|string|A pokemonId|required|
|rightTrainerId|string|A trainerId|required|

---

## Update Pokemon Stats
Endpoint - update/{pokemonId}  
Method - PUT  
Response Type - PokemonModel  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|experience|int|Total experience|optional, >=0|
|hpAdded|int|HP Added Value|optional, >=0|
|attackAdded|int|AttackAdded Value|optional, >=0|
|defenseAdded|int|Defense Added Value|optional, >=0|
|specialAttackAdded|int|Special Attack Added Value|optional, >=0|
|specialDefenseAdded|int|Special Defense Added Value|optional, >=0|
|speedAdded|int|Speed Added Value|optional, >=0|
|nickname|string|The pokemon's nickname|optional|

---

## Delete Pokemon by Id
Endpoint - {pokemonId}  
Method - DELETE  
Response Type - message  
[Back to Pokemon](#pokemon)

---

## Evolve Pokemon
Endpoint - evolve/{pokemonId}  
Method - PUT  
Response Type - PokemonModel  
[Back to Pokemon](#pokemon)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|nextForm|string|Evolution species namence|required|

---

# Trainer
*Resource - api/v1/trainer*  
[Back to top](#Table-Of-Contents)
- [Verify Login](#verify-login)
- [Verify Logout](#verify-logout)
- [Find Trainer Pokemon](#find-trainer-pokemon)
- [Add Trainer Pokemon](#add-trainer-pokemon)
- [Add Items](#add-items)
- [Remove Items](#remove-items)
- [Delete Trainer](#delete-trainer)

---

## Verify Login
Endpoint - login  
Method - GET  
Response Type - TrainerModel  
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
Method - GET  
Response Type - Status(200)  
[Back to Trainer](#trainer)

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
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerId|string|Trainer Id|required|
|pokemon|string|A pokemon's name|required|
|nature|string|A pokemon's nature|required|
|naturalMoves|list|A list of Pokemon moves|At least one, no more than four|
|expYield|int|Experience yield|>0|
|catchRate|int|A pokemon's catch rate|>=0|
|experience|int|Total experience|>=0|
|level|int|Current level|>0|
|tmMoves|list|A list of Pokemon moves|optional, no more than four|
|nickname|string|The pokemon's nickname|optional|

---

## Add Items
Endpoint - {trainerId}/addItems  
Method - PUT  
Response Type - PokemonModel  
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|{itemName}|int|>0|at least one item|

---

## Remove Items
Endpoint - {trainerId}/addItems  
Method - PUT  
Response Type - PokemonModel  
[Back to Trainer](#trainer)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|{itemName}|int|>0|at least one item|

---

## Delete Trainer
Endpoint - {trainerId}  
Method - DELETE  
Response Type - message  
[Back to Trainer](#trainer)

---

# Game
*Resource - api/v1/game*  
[Back to top](#Table-Of-Contents)
* [Find Game by Id](#find-game-by-id)
* [Find Trainer In Game](#find-trainer-in-game)
* [Create New Game](#create-new-game)
* [Add New Trainer to Game](#add-new-trainer-to-game)
* [Import Game](#import-game)
* [Start Game](#start-game)
* [End Game](#end-game)
* [Add Npcs to Game](#add-npcs-to-game)
* [Remove Npcs from Game](#remove-npcs-from-game)
* [Reset Trainer password](#reset-trainer-password)
* [Delete Game](#delete-game)
* [Export Game](#export-game)

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
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|nickname|string|a nickname for the game session|optional|
|gmUsername|string|GM's username|required|
|gmPassword|string|GM's password|required|
|gameSessionPassword|string|the password for the game session|required|

---

## Import Game
Endpoint - import  
Method - POST  
Content Type - application/octet-stream  
Response Type - Ok  

Expects a .json file matching the ExportedGame schema

---

## Add New Trainer to Game
Endpoint - {gameId}/new  
Method - PUT  
Response Type - TrainerModel  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerName|string|Trainer's username|required|
|password|string|Trainer's password|required|

---

## Start Game
Endpoint - {gameId}/start  
Method - PUT  
Response Type - Status(200)  
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
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|trainerId|string|GameMaster's id|required|

---

## Add Npcs to Game
Endpoint - {gameId}/addNpcs  
Method - PUT  
Response Type - Status(200)  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|npcId|string[]|npcs to add to the game|required|

---

## Remove Npcs from Game
Endpoint - {gameId}/removeNpcs  
Method - PUT  
Response Type - Status(200)  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
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
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameSessionPassword|string|Game's password|required|

---

## Export Game
Endpoint -{gameId}/export  
Method - Delete  
Response Type - Ok  
[Back to Game](#game)

|Parameter|Type|Expected Value|Required|
|---------|----|--------------|--------|
|gameSessionPassword|string|Game's password|required|

---

# Npc
*Resource - api/v1/npc*  
[Back to top](#Table-Of-Contents)
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