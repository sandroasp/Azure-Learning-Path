# How to get Key-values from Azure App Configuration within Logic Apps
App Configuration complements Azure Key Vault, which is used to store application secrets. App Configuration makes it easier to implement the following scenarios:
* Centralize management and distribution of hierarchical configuration data for different environments and geographies
* Dynamically change application settings without the need to redeploy or restart an application
* Control feature availability in real-time

The only problem was that unlike Key Vault, which has an available connector to be used inside Logic Apps, App Configuration doesn’t have a connector available.

This Function App is intended to close this gap and for you to be able to use it inside Logic Apps (or any other resource) and read App Configurations.