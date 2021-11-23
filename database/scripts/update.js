// use PTA

// adding missing tables
currentCollections = db.getCollectionNames();
updatedCollections = [
    'Game',
    'NPC',
    'Pokemon',
    'Trainer',
    'Logs'
];
for (collection of updatedCollections){
    if (!currentCollections.includes(collection)){
        console.log(`Adding new collection ${collection}`);
        db.createCollection(collection);
    }
}

// update schema validation
console.log("Updating schema for Game colllection");
db.runCommand({
    collMod: "Game",
    validator: {
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
                    bsonType: 'string',
                    minLength: 36,
                    maxLength: 36
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
                        bsonType: 'string',
                        minLength: 36,
                        maxLength: 36
                    }
                }
            }
        }
    }
});
console.log("Updating schema for NPC collection");
db.runCommand({
    collMod: "NPC",
    validator: {
        $jsonSchema: {
            required: [
                'NPCId',
                'TrainerName',
                'TrainerClasses',
                'TrainerStats',
                'Feats'
            ],
            additionalProperties: false,
            properties: {
                _id: {
                    bsonType: 'objectId'
                },
                NPCId: {
                    bsonType: 'string',
                    minLength: 36,
                    maxLength: 36
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
                Feats: {
                    bsonType: 'array',
                    items: {
                        bsonType: 'string'
                    },
                    maxItems: 36
                }
            }
        }
    }
})
console.log("Updating schema for Pokemon collection");
db.runCommand({
    collMod: "Pokemon",
    validator: {
        $jsonSchema: {
            required: [
            'PokemonId',
            'DexNo',
            'TrainerId',
            'Nickname',
            'Ability',
            'PokemonStatus',
            'Gender',
            'NaturalMoves',
            'TMMoves',
            'Type',
            'Experience',
            'Level',
            'ExpYield',
            'CatchRate',
            'Nature',
            'IsShiny',
            'IsOnActiveTeam',
            'HP',
            'Attack',
            'Defense',
            'SpecialAttack',
            'SpecialDefense',
            'Speed'
            ],
            additionalProperties: false,
            properties: {
                _id: {
                    bsonType: 'objectId'
                },
                PokemonId: {
                    bsonType: 'string',
                    minLength: 36,
                    maxLength: 36
                },
                DexNo: {
                    bsonType: 'int',
                    minimum: 1
                },
                TrainerId: {
                    bsonType: 'string',
                    minLength: 36,
                    maxLength: 36
                },
                Gender: {
                    bsonType: 'int',
                    minimum: 0,
                    maximum: 2
                },
                PokemonStatus: {
                    bsonType: 'int',
                    minimum: -1,
                    maximum: 6
                },
                Nickname: {
                    bsonType: 'string',
                    minLength: 1,
                    maxLength: 18
                },
                Ability: {
                    bsonType: 'int',
                    minimum: 1,
                    maximum: 3
                },
                NaturalMoves: {
                    bsonType: 'array',
                    minItems: 1,
                    maxItems: 4,
                    items: {
                        bsonType: 'string'
                    }
                },
                TMMoves: {
                    bsonType: 'array',
                    maxItems: 4,
                    items: {
                        bsonType: 'string'
                    }
                },
                Type: {
                    bsonType: 'int'
                },
                Experience: {
                    bsonType: 'int',
                    minimum: 0
                },
                Level: {
                    bsonType: 'int',
                    minimum: 1,
                    maximum: 100
                },
                ExpYield: {
                    bsonType: 'int',
                    minimum: 1
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
                Nature: {
                    bsonType: 'int',
                    minimum: 1,
                    maximum: 35
                },
                HP: {
                    bsonType: 'object',
                    required: [
                        'Base',
                        'Modifier',
                        'Added',
                        'Total'
                    ],
                    properties: {
                        Base: {
                            bsonType: 'int'
                        },
                        Modifier: {
                            bsonType: 'int'
                        },
                        Added: {
                            bsonType: 'int'
                        },
                        Total: {
                            bsonType: 'int'
                        }
                    }
                },
                Attack: {
                    bsonType: 'object',
                    required: [
                        'Base',
                        'Modifier',
                        'Added',
                        'Total'
                    ],
                    properties: {
                        Base: {
                            bsonType: 'int'
                        },
                        Modifier: {
                            bsonType: 'int'
                        },
                        Added: {
                            bsonType: 'int'
                        },
                        Total: {
                            bsonType: 'int'
                        }
                    }
                },
                Defense: {
                    bsonType: 'object',
                    required: [
                        'Base',
                        'Modifier',
                        'Added',
                        'Total'
                    ],
                    properties: {
                        Base: {
                            bsonType: 'int'
                        },
                        Modifier: {
                            bsonType: 'int'
                        },
                        Added: {
                            bsonType: 'int'
                        },
                        Total: {
                            bsonType: 'int'
                        }
                    }
                },
                SpecialAttack: {
                    bsonType: 'object',
                    required: [
                        'Base',
                        'Modifier',
                        'Added',
                        'Total'
                    ],
                    properties: {
                        Base: {
                            bsonType: 'int'
                        },
                        Modifier: {
                            bsonType: 'int'
                        },
                        Added: {
                            bsonType: 'int'
                        },
                        Total: {
                            bsonType: 'int'
                        }
                    }
                },
                SpecialDefense: {
                    bsonType: 'object',
                    required: [
                        'Base',
                        'Modifier',
                        'Added',
                        'Total'
                    ],
                    properties: {
                        Base: {
                            bsonType: 'int'
                        },
                        Modifier: {
                            bsonType: 'int'
                        },
                        Added: {
                            bsonType: 'int'
                        },
                        Total: {
                            bsonType: 'int'
                        }
                    }
                },
                Speed: {
                    bsonType: 'object',
                    required: [
                        'Base',
                        'Modifier',
                        'Added',
                        'Total'
                    ],
                    properties: {
                        Base: {
                            bsonType: 'int'
                        },
                        Modifier: {
                            bsonType: 'int'
                        },
                        Added: {
                            bsonType: 'int'
                        },
                        Total: {
                            bsonType: 'int'
                        }
                    }
                }
            }
        }
    }
});
console.log("Updating schema for Trainer collection");
db.runCommand({
    collMod: "Trainer",
    validator: {
        $jsonSchema: {
            required: [
                'GameId',
                'TrainerId',
                'Level',
                'TrainerName',
                'PasswordHash',
                'TrainerClasses',
                'TrainerStats',
                'Feats',
                'Money',
                'IsOnline',
                'Items',
                'IsGM'
            ],
            additionalProperties: false,
            properties: {
                _id: {
                    bsonType: 'objectId'
                },
                GameId: {
                    bsonType: 'string'
                },
                TrainerId: {
                    bsonType: 'string'
                },
                Level: {
                    bsonType: 'int',
                    minimum: 0
                },
                TrainerName: {
                    bsonType: 'string'
                },
                PasswordHash: {
                    bsonType: 'string',
                    minLength: 1
                },
                ActivityToken: {
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
                    properties: {
                        RawStrValue: {
                            bsonType: 'int'
                        },
                        RawDexValue: {
                            bsonType: 'int'
                        },
                        RawConValue: {
                            bsonType: 'int'
                        },
                        RawIntValue: {
                            bsonType: 'int'
                        },
                        RawWisValue: {
                            bsonType: 'int'
                        },
                        RawChaValue: {
                            bsonType: 'int'
                        },
                        EarnedStats: {
                            bsonType: 'int'
                        }
                    }
                },
                Feats: {
                    bsonType: 'array',
                    items: {
                        bsonType: 'string'
                    },
                    maxItems: 36
                },
                Money: {
                    bsonType: 'int'
                },
                IsOnline: {
                    bsonType: 'bool'
                },
                Items: {
                    bsonType: 'array',
                    items: {
                        bsonType: 'object',
                        properties: {
                            Name: {
                                bsonType: 'string',
                                minLength: 1
                            },
                            Amount: {
                                bsonType: 'int',
                                minimum: 1
                            }
                        }
                    }
                },
                IsGM: {
                    bsonType: 'bool'
                }
            }
        }
    }
});
console.log("Updating schema for Logs collection");
db.runCommand({
    collMod: "Logs",
    validator: {
        $jsonSchema: {
            properties: {
                _id: {
                    bsonType: 'objectId'
                }
            }
        }
    }
});