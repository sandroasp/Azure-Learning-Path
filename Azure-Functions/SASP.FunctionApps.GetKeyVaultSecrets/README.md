# How to retrieve Azure Key Vault Secrets using Azure Functions
Azure Key Vault is a cloud service for securely storing and accessing secrets used by your cloud applications. A secret is anything that you want to tightly control access to, such as API keys, passwords, certificates, or cryptographic keys. 

There may be several examples or reasons we want to access the Key Vault and retrieve Secrets using an Azure Function. And the goal of this blog post is not to address all of these reasons. In my case, I'm migrating a Logic App Consumption to Standard, and I found a lack of API connection parity between these two offers. If you are using Logic App Consumption, you will have at your disposal the Azure Key Vault connector where you can:
 * Decrypt or Encrypt data using the latest version of a key.
 * Decrypt or Encrypt data using a specific version of a key.
 * Gets metadata of a key.
 * Gets metadata of a version of a key.
 * Gets a secret.
 * Gets metadata of a secret.
 * Gets a version of a secret.
 * Gets metadata of a version of a secret.
 * List versions of a key.
 * List keys.
 * or List secrets.

However, in Logic App Standard, this connector is not available as a built-in connector. You can always use the managed connector, the Azure Key Vault connector. However, if you are trying to access a Key Vault in a Landing Zone protected by VNET, you will not be able to use the managed connector. The alternatives are:
 * Use the HTTP Connector using the REST API to access the Key Vault
 * Create an API App to access and retrieve Key Vault Secrets
 * Or create an Azure Function to access and retrieve Key Vault Secrets 

In some cases, we can probably use an operation inside API Management to retrieve Key Vault Secrets.

This sample show us how we can retrive a secret or a list of secrets using an Azure Function 

# About Me
**Sandro Pereira** | [DevScope](http://www.devscope.net/) | MVP & MCTS BizTalk Server 2010 | [https://blog.sandro-pereira.com/](https://blog.sandro-pereira.com/) | [@sandro_asp](https://twitter.com/sandro_asp)