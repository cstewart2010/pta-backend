logCollection = db.getCollection("Logs")
logs = logCollection.find()
logs.forEach(console.log)
logs.deleteMany({})