PTA BackEnd 
---

[To Wiki](../../wikis)
* Download and unzip the latest artifacts from http://ptatest.eastus.cloudapp.azure.com:8080/job/PTA%20backend%20develop%20build/
* Run the install.ps1 script to install all necessary applications to get the PTA Backend up and running
  * You will be prompted to create a hash key for the application if this is is a first time install.
* Run the MongoDBImportTool.exe to add all of the static collections
* Run the Core API (default url is "http://localhost:5000/api")
  * Check the /api endpoint to verify that the application is running.
