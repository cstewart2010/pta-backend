logCollection = db.getCollection("Logs")
logCollection.find().forEach(console.log)
logCollection.deleteMany({})