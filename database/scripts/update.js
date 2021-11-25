// use PTA

// adding missing tables
currentCollections = db.getCollectionNames();
const updatedValidators = {
  Games: {
    "$jsonSchema": {
      "required": [
        "GameId",
        "Nickname",
        "IsOnline",
        "PasswordHash",
        "NPCs"
      ],
      "additionalProperties": false,
      "properties": {
      "_id": {
        "bsonType": "objectId"
      },
      "GameId": {
        "bsonType": "string",
        "minLength": 36,
        "maxLength": 36
      },
      "Nickname": {
        "bsonType": "string",
        "minLength": 1,
        "maxLength": 18
      },
      "IsOnline": {
        "bsonType": "bool"
      },
      "PasswordHash": {
        "bsonType": "string",
        "minLength": 1
      },
      "NPCs": {
        "bsonType": "array",
        "items": {
        "bsonType": "string",
        "minLength": 36,
        "maxLength": 36
        }
      }
      }
    }
  },
  NPCs: {
    "$jsonSchema": {
      "required": [
        "NPCId",
        "TrainerName",
        "TrainerClasses",
        "TrainerStats",
        "Feats"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "NPCId": {
          "bsonType": "string",
          "minLength": 36,
          "maxLength": 36
        },
        "TrainerName": {
          "bsonType": "string",
          "minLength": 1,
          "maxLength": 18
        },
        "TrainerClasses": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          },
          "maxItems": 4
        },
        "TrainerStats": {
          "bsonType": "object"
        },
        "Feats": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          }
        }
      }
    }
  },
  Pokemon: {
    "$jsonSchema": {
      "required": [
        "PokemonId",
        "DexNo",
        "SpeciesName",
        "TrainerId",
        "Nickname",
        "Gender",
        "PokemonStatus",
        "Moves",
        "Type",
        "CatchRate",
        "Nature",
        "IsShiny",
        "IsOnActiveTeam",
        "PokemonStats",
        "Size",
        "Weight",
        "Rarity",
        "Skills",
        "Passives",
        "EggGroups",
        "EggHatchRate",
        "Diet",
        "Habitats",
        "Proficiencies"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "PokemonId": {
          "bsonType": "string",
          "minLength": 36,
          "maxLength": 36
        },
        "DexNo": {
          "bsonType": "int",
          "minimum": 1
        },
        "TrainerId": {
          "bsonType": "string",
          "minLength": 36,
          "maxLength": 36
        },
        "Gender": {
          "bsonType": "int",
          "minimum": 0,
          "maximum": 2
        },
        "PokemonStatus": {
          "bsonType": "int",
          "minimum": -1,
          "maximum": 6
        },
        "Nickname": {
          "bsonType": "string",
          "minLength": 1,
          "maxLength": 18
        },
        "Ability": {
          "bsonType": "int",
          "minimum": 1,
          "maximum": 3
        },
        "Moves": {
          "bsonType": "array",
          "minItems": 1,
          "maxItems": 6,
          "items": {
            "bsonType": "string"
          }
        },
        "Type": {
          "bsonType": "int"
        },
        "CatchRate": {
          "bsonType": "int",
          "minimum": 0,
          "maximum": 255
        },
        "IsOnActiveTeam": {
          "bsonType": "bool"
        },
        "IsShiny": {
          "bsonType": "bool"
        },
        "Nature": {
          "bsonType": "int",
          "minimum": 1,
          "maximum": 20
        },
        "PokemonStats": {
          "bsonType": "object",
          "required": [
            "HP",
            "Attack",
            "Defense",
            "SpecialAttack",
            "SpecialDefense",
            "Speed"
          ],
          "properties": {
            "HP": {
              "bsonType": "int",
              "minimum": 1
            },
            "Attack": {
              "bsonType": "int",
              "minimum": 1
            },
            "Defense": {
              "bsonType": "int",
              "minimum": 1
            },
            "SpecialAttack": {
              "bsonType": "int",
              "minimum": 1
            },
            "SpecialDefense": {
              "bsonType": "int",
              "minimum": 1
            },
            "Speed": {
              "bsonType": "int",
              "minimum": 1
            }
          }
        },
        "Size": {
          "bsonType": "string",
          "enum": [
            "Tiny",
            "Small",
            "Medium",
            "Large",
            "Dynamic",
            "Huge",
            "Gigantic"
          ]
        },
        "Weight": {
          "bsonType": "string",
          "enum": [
            "Featherweight",
            "Light",
            "Medium",
            "Heavy",
            "Dynamic",
            "Superweight"
          ]
        },
        "Skills": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          }
        },
        "Passives": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          }
        },
        "Proficiencies": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          }
        },
        "EggGroups": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          }
        },
        "EggHatchRate": {
          "bsonType": "string",
          "minLength": 1
        },
        "Habitats": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          }
        },
        "Diet": {
          "bsonType": "string",
          "minLength": 1
        },
        "Rarity": {
          "bsonType": "string",
          "enum": [
            "Common",
            "Uncommon",
            "Rare"
          ]
        },
        "SpecialFormName": {
          "bsonType": "string",
          "enum": [
            "Mega",
            "Gigantamax"
          ]
        },
        "BaseFormName": {
          "bsonType": "string",
          "minLength": 1
        },
        "GMaxMove": {
          "bsonType": "string",
          "minLength": 1
        },
        "EvolvesFrom": {
          "bsonType": "string",
          "minLength": 1
        },
        "LegendaryStats": {
          "bsonType": "object",
          "additionalProperties": false,
          "properties": {
            "HP": {
              "bsonType": "int",
              "minimum": 1
            },
            "Moves": {
              "bsonType": "array",
              "items": {
                "bsonType": "string"
              }
            },
            "LegendaryMoves": {
              "bsonType": "array",
              "items": {
                "bsonType": "string"
              }
            },
            "Passives": {
              "bsonType": "array",
              "items": {
                "bsonType": "string"
              }
            },
            "Features": {
              "bsonType": "array",
              "items": {
                "bsonType": "string"
              }
            }
          }
        }
      }
    }
  },
  Trainers: {
    "$jsonSchema": {
      "required": [
        "GameId",
        "TrainerId",
        "Honors",
        "TrainerName",
        "PasswordHash",
        "TrainerClasses",
        "TrainerStats",
        "Feats",
        "Money",
        "IsOnline",
        "Items",
        "IsGM",
        "Origin"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "GameId": {
          "bsonType": "string"
        },
        "TrainerId": {
          "bsonType": "string"
        },
        "Honors": {
          "bsonType": "array",
          "items": {
            "bsonType": "string",
            "minLength": 1
          }
        },
        "TrainerName": {
          "bsonType": "string"
        },
        "PasswordHash": {
          "bsonType": "string",
          "minLength": 1
        },
        "ActivityToken": {
          "bsonType": "string"
        },
        "TrainerClasses": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          },
          "maxItems": 4
        },
        "TrainerStats": {
          "bsonType": "object",
          "required": [
            "HP",
            "Attack",
            "Defense",
            "SpecialAttack",
            "SpecialDefense",
            "Speed"
          ],
          "properties": {
            "HP": {
              "bsonType": "int",
              "minimum": 20,
              "maximum": 32
            },
            "Attack": {
              "bsonType": "int",
              "minimum": 1
            },
            "Defense": {
              "bsonType": "int",
              "minimum": 1
            },
            "SpecialAttack": {
              "bsonType": "int",
              "minimum": 1
            },
            "SpecialDefense": {
              "bsonType": "int",
              "minimum": 1
            },
            "Speed": {
              "bsonType": "int",
              "minimum": 1
            }
          }
        },
        "Feats": {
          "bsonType": "array",
          "items": {
            "bsonType": "string"
          }
        },
        "Money": {
          "bsonType": "int"
        },
        "IsOnline": {
          "bsonType": "bool"
        },
        "Items": {
          "bsonType": "array",
          "items": {
            "bsonType": "object",
            "properties": {
              "Name": {
                "bsonType": "string"
              },
              "Effect": {
                "bsonType": "string"
              },
              "Amount": {
                "bsonType": "int",
                "minimum": 1
              }
            }
          }
        },
        "IsGM": {
          "bsonType": "bool"
        },
        "Origin": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  Logs: {
    $jsonSchema: {
      properties: {
        _id: {
          bsonType: 'objectId'
        }
      }
    }
  },
  BasePokemon: {
    $jsonSchema: {
      required: [
        'DexNo',
        'Name',
        'PokemonStats',
        'Type',
        'Size',
        'Weight',
        'Rarity',
        'Skills',
        'Passives',
        'Moves',
        'EggGroups',
        'EggHatchRate',
        'Diet',
        'Habitats',
        'Proficiencies',
        'Stage'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        DexNo: {
          bsonType: 'int'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        PokemonStats: {
          bsonType: 'object',
          required: [
            'HP',
            'Attack',
            'Defense',
            'SpecialAttack',
            'SpecialDefense',
            'Speed'
          ],
          properties: {
            HP: {
              bsonType: 'int',
              minimum: 1
            },
            Attack: {
              bsonType: 'int',
              minimum: 1
            },
            Defense: {
              bsonType: 'int',
              minimum: 1
            },
            SpecialAttack: {
              bsonType: 'int',
              minimum: 1
            },
            SpecialDefense: {
              bsonType: 'int',
              minimum: 1
            },
            Speed: {
              bsonType: 'int',
              minimum: 1
            }
          }
        },
        Moves: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        Type: {
          bsonType: 'string'
        },
        Size: {
          bsonType: 'string',
          'enum': [
            'Tiny',
            'Small',
            'Medium',
            'Large',
            'Dynamic',
            'Huge',
            'Gigantic'
          ]
        },
        Weight: {
          bsonType: 'string',
          'enum': [
            'Featherweight',
            'Light',
            'Medium',
            'Heavy',
            'Dynamic',
            'Superweight'
          ]
        },
        Skills: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        Passives: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        Proficiencies: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        EggGroups: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        EggHatchRate: {
          bsonType: 'string'
        },
        Habitats: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        Diet: {
          bsonType: 'string',
          minLength: 1
        },
        Rarity: {
          bsonType: 'string'
        },
        Stage: {
          bsonType: 'int'
        },
        SpecialFormName: {
          bsonType: 'string',
          'enum': [
            '',
            'Mega',
            'Gigantamax'
          ]
        },
        BaseFormName: {
          bsonType: 'string'
        },
        GMaxMove: {
          bsonType: 'string'
        },
        EvolvesFrom: {
          bsonType: 'string'
        },
        LegendaryStats: {
          bsonType: 'object',
          additionalProperties: false,
          properties: {
            HP: {
              bsonType: 'int'
            },
            Moves: {
              bsonType: 'array',
              items: {
                bsonType: 'string'
              }
            },
            LegendaryMoves: {
              bsonType: 'array',
              items: {
                bsonType: 'string'
              }
            },
            Passives: {
              bsonType: 'array',
              items: {
                bsonType: 'string'
              }
            },
            Features: {
              bsonType: 'array',
              items: {
                bsonType: 'string'
              }
            }
          }
        }
      }
    }
  },
  Berries: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Price",
        "Effects",
        "Flavors",
        "Rarity"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Price": {
            "bsonType": "int"
        },
        "Effects": {
          "bsonType": "string",
          "minLength": 1
        },
        "Flavors": {
            "bsonType": "int"
        },
        "Rarity": {
            "bsonType": "string",
            "enum": [
                "Common",
                "Uncommon",
                "Rare"
            ]
        }
      }
    }
  },
  Features: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Effects"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Effects": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  KeyItems: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Price",
        "Effects"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Price": {
            "bsonType": "int"
        },
        "Effects": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  LegendaryFeatures: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Effect"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Effect": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  MedicalItems: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Price",
        "Effects"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Price": {
            "bsonType": "int"
        },
        "Effects": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  Moves: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Range",
        "Type",
        "Stat",
        "Frequency"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Range": {
          "bsonType": "int",
          "minimum": 0
        },
        "Type": {
          "bsonType": "int"
        },
        "Stat": {
          "bsonType": "string",
          "enum": [
            "Attack",
            "Special Attack",
            "Effect",
            "Variable"
          ]
        },
        "Frequency": {
          "bsonType": "string",
          "minLength": 1
        },
        "DiceRoll": {
          "bsonType": "string",
          "minLength": 1
        },
        "Effect": {
          "bsonType": "string",
          "minLength": 1
        },
        "GrantedSkills": {
          "bsonType": "array",
          "items": {
            "bsonType": "string",
            "minLength": 1
          }
        },
        "ContestStat": {
          "bsonType": "string",
          "enum": [
            "Beauty",
            "Clever",
            "Cool",
            "Cute",
            "Tough"
          ]
        },
        "ContestKeyword": {
          "bsonType": "string",
          "enum": [
            "Appeal",
            "Attention Grabber",
            "Big Show",
            "Catching Up",
            "Crowd Pleaser",
            "End Set",
            "Excitement",
            "Final Appeal",
            "Get Ready!",
            "Good Show!",
            "Hold That Thought",
            "Incredible",
            "Incentives",
            "Interrupting Appeal",
            "Inversed Appeal",
            "Quick Set",
            "Reflective Appeal",
            "Reliable",
            "Round Ender",
            "Round Starter",
            "Scrambler",
            "Seen Nothing Yet",
            "Slow Set",
            "Special Attention",
            "Start Set",
            "Torrential Appeal",
            "Unsettling"
          ]
        }
      }
    }
  },
  Origins: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Skill",
        "Lifestyle",
        "Saving",
        "Equipment",
        "Pokemon",
        "Feature"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Skill": {
          "bsonType": "string",
          "minLength": 1
        },
        "Lifestyle": {
          "bsonType": "string",
          "enum": [
            "Difficult",
            "Modest",
            "Comfortable",
            "Wealthy"
          ]
        },
        "Savings": {
          "bsonType": "int",
          "minimum": 0
        },
        "Equipment": {
          "bsonType": "string",
          "minLength": 1
        },
        "Pokemon": {
          "bsonType": "string",
          "minLength": 1
        },
        "Feature": {
          "bsonType": "object",
          "required": [
            "Name",
            "Effect"
          ],
          "additionalProperties": false,
          "properties": {
            "Name": {
              "bsonType": "string",
              "minLength": 1
            },
            "Effect": {
              "bsonType": "string",
              "minLength": 1
            }
          }
        }
      }
    }
  },
  Passives: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Effect"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Effect": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  Pokeballs: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Price",
        "Effects"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Price": {
            "bsonType": "int"
        },
        "Effects": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  PokemonItems: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Price",
        "Effects"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Price": {
            "bsonType": "int"
        },
        "Effects": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  Skills: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Effect"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Effect": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  },
  TrainerClasses: {
    "$jsonSchema": {
      "required": [
        "Name",
        "IsBaseClass",
        "Feats"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "BaseClass": {
          "bsonType": "string",
          "minLength": 1
        },
        "IsBaseClass": {
          "bsonType": "bool"
        },
        "Feats": {
          "bsonType": "array",
          "items": {
            "bsonType": "object",
            "required": [
              "Name",
              "Frequency",
              "LevelLearned"
            ],
            "properties": {
              "Name": {
                "bsonType": "string",
                "minLength": 1
              },
              "Frequency": {
                "bsonType": "string",
                "minLength": 1
              },
              "LevelLearned": {
                "bsonType": "int",
                "minimum": 1,
                "maximum": 15
              }
            }
          }
        }
      }
    }
  },
  TrainerEquipment: {
    "$jsonSchema": {
      "required": [
        "Name",
        "Price",
        "Effects"
      ],
      "additionalProperties": false,
      "properties": {
        "_id": {
          "bsonType": "objectId"
        },
        "Name": {
          "bsonType": "string",
          "minLength": 1
        },
        "Price": {
            "bsonType": "int"
        },
        "Effects": {
          "bsonType": "string",
          "minLength": 1
        }
      }
    }
  }
}

// update schema validation
for (const collection in updatedValidators) {
  if (!currentCollections.includes(collection)){
    console.log(`Adding new collection ${collection}`);
    db.createCollection(collection);
  }

  console.log(`Updating schema for ${collection} colllection`);
  db.runCommand({
      collMod: collection,
      validator: updatedValidators[collection]
  })
}
