// use PTA

// adding missing tables
currentCollections = db.getCollectionNames();
const updatedValidators = {
  Games: {
    $jsonSchema: {
      required: [
        'GameId',
        'Nickname',
        'IsOnline',
        'PasswordHash',
        'NPCs'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        GameId: {
          bsonType: 'binData'
        },
        Nickname: {
          bsonType: 'string',
          minLength: 1,
          maxLength: 18
        },
        IsOnline: {
          bsonType: 'bool'
        },
        PasswordHash: {
          bsonType: 'string',
          minLength: 1
        },
        NPCs: {
          bsonType: 'array',
          items: {
            bsonType: 'binData'
          }
        },
        Logs: {
          bsonType: 'array',
          items: {
            bsonType: 'object',
            properties: {
              User: {
                bsonType: 'string'
              },
              Action: {
                bsonType: 'string'
              }
            }
          }
        }
      }
    }
  },
  Settings: {
    $jsonSchema: {
      required: [
        'SettingId',
        'GameId',
        'Name',
        'IsActive',
        'Type',
        'ActiveParticipants',
        'Environment',
        'Shops'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        SettingId: {
          bsonType: 'binData'
        },
        GameId: {
          bsonType: 'binData'
        },
        Name: {
          bsonType: 'string',
          minLength: 1,
          maxLength: 36
        },
        IsActive: {
          bsonType: 'bool'
        },
        Type: {
          bsonType: 'string',
          'enum': [
            'Hostile',
            'NonHostile',
            'Hybrid'
          ]
        },
        ActiveParticipants: {
          bsonType: 'array',
          items: {
            bsonType: 'object',
            required: [
              'ParticipantId',
              'Type',
              'Name',
              'Health',
              'Speed',
              'Position'
            ],
            additionalProperties: false,
            properties: {
              ParticipantId: {
                bsonType: 'binData'
              },
              Name: {
                bsonType: 'string'
              },
              Type: {
                bsonType: 'string',
                enum: [
                  'Trainer',
                  'Pokemon',
                  'WildPokemon',
                  'EnemyNpc',
                  'EnemyPokemon',
                  'NeutralNpc',
                  'NeutralPokemon',
                  'Shop'
                ]
              },
              Health: {
                bsonType: 'string'
              },
              Speed: {
                bsonType: 'double'
              },
              Position: {
                bsonType: 'object',
                required: [
                  'X',
                  'Y'
                ],
                additionalProperties: false,
                properties: {
                  X: {
                    bsonType: 'int',
                    minimum: 0,
                    maximum: 20
                  },
                  Y: {
                    bsonType: 'int',
                    minimum: 0,
                    maximum: 20
                  }
                }
              }
            }
          }
        },
        Environment: {
          bsonType: 'array',
          items: {
            enum: [
              'Safari',
              'Rainforest',
              'Cave',
              'Taiga',
              'Artic',
              'Desert',
              'Urban',
              'Freshwater',
              'Beach',
              'Tundra',
              'Grassland',
              'Marsh',
              'NoSunlight',
              'Forest',
              'OnWater',
              'Mountain',
              'InCombat'
            ]
          }
        },
        Shops: {
          bsonType: 'array',
          items: {
            bsonType: 'binData'
          }
        }
      }
    }
  },
  NPCs: {
    $jsonSchema: {
      required: [
        'NPCId',
        'TrainerName',
        'TrainerClasses',
        'TrainerStats',
        'CurrentHP',
        'Feats',
        'GameId',
        'Level',
        'TrainerSkills'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        NPCId: {
          bsonType: 'binData'
        },
        TrainerName: {
          bsonType: 'string',
          minLength: 1,
          maxLength: 18
        },
        TrainerClasses: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          },
          maxItems: 4
        },
        TrainerStats: {
          bsonType: 'object'
        },
        CurrentHP: {
          bsonType: 'int'
        },
        Feats: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        GameId: {
          bsonType: 'binData'
        },
        Level: {
          bsonType: 'int'
        },
        TrainerSkills: {
          bsonType: 'array',
          items: {
            required: [
              'Name',
              'Talent1',
              'Talent2',
              'ModifierStat'
            ],
            additionalProperties: false,
            properties: {
              Name: {
                bsonType: 'string',
                'enum': [
                  'Acrobatics',
                  'Athletics',
                  'Bluff/Deception',
                  'Concentration',
                  'Constitution',
                  'Diplomacy/Persuasion',
                  'Engineering/Operation',
                  'History',
                  'Insight',
                  'Investigation',
                  'Medicine',
                  'Nature',
                  'Perception',
                  'Performance',
                  'Pokémon Handling',
                  'Programming',
                  'Sleight of Hand',
                  'Stealth'
                ]
              },
              Talent1: {
                bsonType: 'bool'
              },
              Talent2: {
                bsonType: 'bool'
              },
              ModifierStat: {
                bsonType: 'string'
              }
            }
          }
        },
        Age: {
          bsonType: 'int'
        },
        Gender: {
          bsonType: 'string'
        },
        Height: {
          bsonType: 'int'
        },
        Weight: {
          bsonType: 'int'
        },
        Description: {
          bsonType: 'string'
        },
        Personality: {
          bsonType: 'string'
        },
        Background: {
          bsonType: 'string'
        },
        Goals: {
          bsonType: 'string'
        },
        Species: {
          bsonType: 'string'
        },
        Sprite: {
          bsonType: 'string'
        }
      }
    }
  },
  Pokemon: {
    $jsonSchema: {
      required: [
        'PokemonId',
        'DexNo',
        'Form',
        'AlternateForms',
        'NormalPortrait',
        'ShinyPortrait',
        'SpeciesName',
        'OriginalTrainerId',
        'TrainerId',
        'GameId',
        'Nickname',
        'Gender',
        'PokemonStatus',
        'CurrentHP',
        'Moves',
        'Type',
        'CatchRate',
        'Nature',
        'IsShiny',
        'IsOnActiveTeam',
        'CanEvolve',
        'PokemonStats',
        'Size',
        'Weight',
        'Rarity',
        'Skills',
        'Passives',
        'EggGroups',
        'EggHatchRate',
        'Diet',
        'Habitats',
        'Proficiencies',
        'Pokeball'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        PokemonId: {
          bsonType: 'binData'
        },
        DexNo: {
          bsonType: 'int',
          minimum: 1
        },
        Form: {
          bsonType: 'string'
        },
        AlternateForms: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        NormalPortrait: {
          bsonType: 'string'
        },
        ShinyPortrait: {
          bsonType: 'string'
        },
        SpeciesName: {
          bsonType: 'string',
          minLength: 1
        },
        OriginalTrainerId: {
          bsonType: 'binData'
        },
        TrainerId: {
          bsonType: 'binData'
        },
        GameId: {
          bsonType: 'binData'
        },
        Gender: {
          bsonType: 'string',
          'enum': [
            'Male',
            'Female',
            'Genderless'
          ]
        },
        PokemonStatus: {
          bsonType: 'string'
        },
        CurrentHP: {
          bsonType: 'int'
        },
        Nickname: {
          bsonType: 'string',
          minLength: 1,
          maxLength: 18
        },
        Moves: {
          bsonType: 'array',
          minItems: 1,
          maxItems: 6,
          items: {
            bsonType: 'string'
          }
        },
        Type: {
          bsonType: 'string'
        },
        CatchRate: {
          bsonType: 'int',
          minimum: 0,
          maximum: 255
        },
        IsOnActiveTeam: {
          bsonType: 'bool'
        },
        IsShiny: {
          bsonType: 'bool'
        },
        CanEvolve: {
          bsonType: 'bool'
        },
        Nature: {
          bsonType: 'string',
          'enum': [
            'Lonely',
            'Brave',
            'Adamant',
            'Naughty',
            'Bold',
            'Relaxed',
            'Impish',
            'Lax',
            'Timid',
            'Hasty',
            'Jolly',
            'Naive',
            'Modest',
            'Mild',
            'Quiet',
            'Rash',
            'Calm',
            'Gentle',
            'Sassy',
            'Careful'
          ]
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
        Pokeball: {
          bsonType: 'string',
          enum: [
            'BasicBall',
            'ParkBall',
            'CherishBall',
            'PremierBall',
            'SportBall',
            'HeavyBall',
            'LevelBall',
            'NestBall',
            'RainforestBall',
            'GreatBall',
            'SafariBall',
            'LuxuryBall',
            'LureBall',
            'HeatBall',
            'CaveBall',
            'EarthBall',
            'FineBall',
            'TaigaBall',
            'SaveBall',
            'ArticBall',
            'DesertBall',
            'HauntBall',
            'UrbanBall',
            'NetBall',
            'FreshwaterBall',
            'BeachBall',
            'TimerBall',
            'MysticBall',
            'AirBall',
            'FastBall',
            'UltraBall',
            'HealBall',
            'MasterBall',
            'TundraBall',
            'FriendBall',
            'GrasslandBall',
            'MarshBall',
            'QuickBall',
            'RepeatBall',
            'DreamBall',
            'MoonBall',
            'DuskBall',
            'MoldBall',
            'SolidBall',
            'ForestBall',
            'LoveBall',
            'MountainBall'
          ]
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
          bsonType: 'string',
          minLength: 1
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
          bsonType: 'string',
          'enum': [
            'Common',
            'Uncommon',
            'Rare'
          ]
        },
        GMaxMove: {
          bsonType: 'string'
        },
        EvolvedFrom: {
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
  Trainers: {
    $jsonSchema: {
      required: [
        'GameId',
        'TrainerId',
        'Honors',
        'TrainerName',
        'TrainerClasses',
        'TrainerStats',
        'CurrentHP',
        'Feats',
        'Money',
        'IsOnline',
        'Items',
        'IsGM',
        'IsAllowed',
        'Origin',
        'TrainerSkills'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        GameId: {
          bsonType: 'binData'
        },
        TrainerId: {
          bsonType: 'binData'
        },
        Honors: {
          bsonType: 'array',
          items: {
            bsonType: 'string',
            minLength: 1
          }
        },
        TrainerName: {
          bsonType: 'string'
        },
        TrainerClasses: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          },
          maxItems: 4
        },
        TrainerStats: {
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
              minimum: 20,
              maximum: 32
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
        CurrentHP: {
          bsonType: 'int'
        },
        Feats: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        Money: {
          bsonType: 'int'
        },
        IsOnline: {
          bsonType: 'bool'
        },
        Items: {
          bsonType: 'array',
          additionalProperties: false,
          items: {
            bsonType: 'object',
            properties: {
              Name: {
                bsonType: 'string'
              },
              Effects: {
                bsonType: 'string'
              },
              Amount: {
                bsonType: 'int',
                minimum: 1
              },
              Type: {
                bsonType: 'string',
                enum: [
                  'Berry',
                  'Key',
                  'Medical',
                  'Pokeball',
                  'Pokemon',
                  'Trainer'
                ]
              }
            }
          }
        },
        IsGM: {
          bsonType: 'bool'
        },
        IsAllowed: {
          bsonType: 'bool'
        },
        Origin: {
          bsonType: 'string'
        },
        IsComplete: {
          bsonType: 'bool'
        },
        TrainerSkills: {
          bsonType: 'array',
          items: {
            required: [
              'Name',
              'Talent1',
              'Talent2',
              'ModifierStat'
            ],
            additionalProperties: false,
            properties: {
              Name: {
                bsonType: 'string',
                'enum': [
                  'Acrobatics',
                  'Athletics',
                  'Bluff/Deception',
                  'Concentration',
                  'Constitution',
                  'Diplomacy/Persuasion',
                  'Engineering/Operation',
                  'History',
                  'Insight',
                  'Investigation',
                  'Medicine',
                  'Nature',
                  'Perception',
                  'Performance',
                  'Pokémon Handling',
                  'Programming',
                  'Sleight of Hand',
                  'Stealth'
                ]
              },
              Talent1: {
                bsonType: 'bool'
              },
              Talent2: {
                bsonType: 'bool'
              },
              ModifierStat: {
                bsonType: 'string'
              }
            }
          }
        },
        Age: {
          bsonType: 'int'
        },
        Sprite: {
          bsonType: 'string'
        },
        Gender: {
          bsonType: 'string'
        },
        Height: {
          bsonType: 'int'
        },
        Weight: {
          bsonType: 'int'
        },
        Description: {
          bsonType: 'string'
        },
        Personality: {
          bsonType: 'string'
        },
        Background: {
          bsonType: 'string'
        },
        Goals: {
          bsonType: 'string'
        },
        Species: {
          bsonType: 'string'
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
  PokeDex:{
    $jsonSchema: {
      required: [
        'TrainerId',
        'DexNo',
        'IsSeen',
        'IsCaught',
        'GameId'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        TrainerId: {
          bsonType: 'binData'
        },
        GameId: {
          bsonType: 'binData'
        },
        DexNo: {
          bsonType: 'int'
        },
        IsSeen: {
          bsonType: 'bool'
        },
        IsCaught: {
          bsonType: 'bool'
        }
      }
    }
  },
  Shops: {
    $jsonSchema: {
      required: [
        'ShopId',
        'GameId',
        'IsActive',
        'Inventory',
        'Name'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 6,
          maxLength: 16
        },
        ShopId: {
          bsonType: 'binData'
        },
        GameId: {
          bsonType: 'binData'
        },
        IsActive: {
          bsonType: 'bool'
        },
        Inventory: {
          bsonType: 'object',
          additionalProperties: false,
          patternProperties: {
            '^\\w+( +\\w+)*$': {
              additionalProperties: false,
              required: [
                'Cost',
                'Effects',
                'Type',
                'Quantity'
              ],
              properties: {
                Cost: {
                  bsonType: 'int',
                  minimum: 1
                },
                Effects: {
                  bsonType: 'string'
                },
                Type: {
                  bsonType: 'string',
                  'enum': [
                    'Key',
                    'Medical',
                    'Pokeball',
                    'Pokemon',
                    'Trainer'
                  ]
                },
                Quantity: {
                  bsonType: 'int',
                  minimum: -1
                }
              }
            }
          }
        }
      }
    }
  },
  BasePokemon: {
    $jsonSchema: {
      required: [
        'DexNo',
        'Form',
        'NormalPortrait',
        'ShinyPortrait',
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
        Form: {
          bsonType: 'string'
        },
        NormalPortrait: {
          bsonType: 'string'
        },
        ShinyPortrait: {
          bsonType: 'string'
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
    $jsonSchema: {
      required: [
        'Name',
        'Price',
        'Effects',
        'Flavors',
        'Rarity'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Price: {
          bsonType: 'int'
        },
        Effects: {
          bsonType: 'string'
        },
        Flavors: {
          bsonType: 'string'
        },
        Rarity: {
          bsonType: 'string',
          'enum': [
            'Common',
            'Uncommon',
            'Rare'
          ]
        }
      }
    }
  },
  Features: {
    $jsonSchema: {
      required: [
        'Name',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  KeyItems: {
    $jsonSchema: {
      required: [
        'Name',
        'Price',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Price: {
          bsonType: 'int'
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  LegendaryFeatures: {
    $jsonSchema: {
      required: [
        'Name',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  MedicalItems: {
    $jsonSchema: {
      required: [
        'Name',
        'Price',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Price: {
          bsonType: 'int'
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  Moves: {
    $jsonSchema: {
      required: [
        'Name',
        'Range',
        'Type',
        'Stat',
        'Frequency'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Range: {
          bsonType: 'string'
        },
        Type: {
          bsonType: 'string'
        },
        Stat: {
          bsonType: 'string',
          'enum': [
            'Attack',
            'Special',
            'Effect',
            '(Variable)',
            'Varies'
          ]
        },
        Frequency: {
          bsonType: 'string',
          minLength: 1
        },
        DiceRoll: {
          bsonType: 'string'
        },
        Effects: {
          bsonType: 'string'
        },
        GrantedSkills: {
          bsonType: 'array',
          items: {
            bsonType: 'string'
          }
        },
        ContestStat: {
          bsonType: 'string',
          'enum': [
            '',
            'Beauty',
            'Clever',
            'Cool',
            'Cute',
            'Tough'
          ]
        },
        ContestKeyword: {
          bsonType: 'string',
          'enum': [
            '',
            'Appeal',
            'Attention Grabber',
            'Big Show',
            'Catching Up',
            'Crowd Pleaser',
            'End Set',
            'Excitement',
            'Final Appeal',
            'Get Ready!',
            'Good Show!',
            'Hold That Thought',
            'Incredible',
            'Incentives',
            'Interrupting Appeal',
            'Inversed Appeal',
            'Quick Set',
            'Reflective Appeal',
            'Reliable',
            'Round Ender',
            'Round Starter',
            'Scrambler',
            'Seen Nothing Yet',
            'Slow Set',
            'Special Attention',
            'Start Set',
            'Torrential Appeal',
            'Unsettling'
          ]
        }
      }
    }
  },
  Origins: {
    $jsonSchema: {
      required: [
        'Name',
        'Skill',
        'Lifestyle',
        'Savings',
        'Equipment',
        'StartingPokemon',
        'Feature',
        'StartingEquipmentList'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Skill: {
          bsonType: 'string',
          minLength: 1
        },
        Lifestyle: {
          bsonType: 'string',
          'enum': [
            'Difficult',
            'Modest',
            'Comfortable',
            'Wealthy',
            'Special',
            'Variable',
            'You just hatched!'
          ]
        },
        Savings: {
          bsonType: 'int',
          minimum: 0
        },
        Equipment: {
          bsonType: 'string'
        },
        StartingPokemon: {
          bsonType: 'string',
          minLength: 1
        },
        Feature: {
          bsonType: 'object',
          required: [
            'Name',
            'Effects'
          ],
          additionalProperties: false,
          properties: {
            _id: {
              bsonType: 'objectId'
            },
            Name: {
              bsonType: 'string',
              minLength: 1
            },
            Effects: {
              bsonType: 'string',
              minLength: 1
            }
          }
        },
        StartingEquipmentList: {
          bsonType: 'array',
          items: {
            bsonType: 'object',
            additionalProperties: false,
            properties: {
              Name: {
                bsonType: 'string'
              },
              Type: {
                bsonType: 'int'
              },
              Amount: {
                bsonType: 'int'
              }
            }
          }
        }
      }
    }
  },
  Passives: {
    $jsonSchema: {
      required: [
        'Name',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  Pokeballs: {
    $jsonSchema: {
      required: [
        'Name',
        'Price',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Price: {
          bsonType: 'int'
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  PokemonItems: {
    $jsonSchema: {
      required: [
        'Name',
        'Price',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Price: {
          bsonType: 'int'
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  Skills: {
    $jsonSchema: {
      required: [
        'Name',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  TrainerClasses: {
    $jsonSchema: {
      required: [
        'Name',
        'BaseClass',
        'IsBaseClass',
        'Feats',
        'PrimaryStat',
        'SecondaryStat',
        'Skills'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        BaseClass: {
          bsonType: 'string'
        },
        IsBaseClass: {
          bsonType: 'bool'
        },
        Feats: {
          bsonType: 'array',
          items: {
            bsonType: 'object',
            additionalProperties: false,
            properties: {
              Name: {
                bsonType: 'string',
                minLength: 1
              },
              LevelLearned: {
                bsonType: 'int',
                minimum: 1,
                maximum: 15
              }
            }
          }
        },
        PrimaryStat: {
          bsonType: 'string'
        },
        SecondaryStat: {
          bsonType: 'string'
        },
        Skills: {
          bsonType: 'string'
        }
      }
    }
  },
  TrainerEquipment: {
    $jsonSchema: {
      required: [
        'Name',
        'Price',
        'Effects'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        Name: {
          bsonType: 'string',
          minLength: 1
        },
        Price: {
          bsonType: 'int'
        },
        Effects: {
          bsonType: 'string'
        }
      }
    }
  },
  Users: {
    $jsonSchema:{
      required:[
        'UserId',
        'Username',
        'PasswordHash',
        'ActivityToken',
        'DateCreated',
        'SiteRole',
        'Games',
        'Messages'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        UserId:{
          bsonType: 'binData'
        },
        Username:{
          bsonType: 'string',
          minLength: 6,
          maxLength: 18
        },
        IsOnline:{
          bsonType: 'bool'
        },
        PasswordHash: {
          bsonType: 'string',
        },
        ActivityToken: {
          bsonType: 'string'
        },
        DateCreated:{
          bsonType: 'string',
        },
        SiteRole:{
          bsonType:'string',
          enum:['IpBanned','Deactivated','Active','SiteAdmin']
        },
        Games:{
          bsonType: 'array',
          items:{
            bsonType: 'binData'
          }
        },
        Messages:{
          bsonType: 'array',
          items:{
            bsonType: 'binData'
          }
        }
      }
    }
  },
  UserMessageThreads: {
    $jsonSchema: {
      required: [
        'MessageId',
        'Messages'
      ],
      additionalProperties: false,
      properties: {
        _id: {
          bsonType: 'objectId'
        },
        MessageId:{
          bsonType: 'binData'
        },
        Messages:{
          bsonType: 'array',
          items:{
            properties:{
              Message:{
                bsonType: 'string',
              },
              User:{
                bsonType: 'string',
              },
              Timestamp:{
                bsonType: 'string',
              }
            },
            additionalProperties: false
          }
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
