# Find Orphaned API Connectors in all Resource Groups
Developing solutions on Azure is sometimes an effortless operation. Administrate all your Azure resources that may be a different story. And this PowerShell script focuses on simplifying one of these processes.

One of the painful processes, not only when we are developing our Logic App, but mainly when we are administrating them, is finding what API Connections and no longer being used by your Logic Apps. What we call Orphaned API Connections.

This PowerShell script will look at all of the API Connections in all resource groups present in a specific Azure Subscription and then inspect every Logic App in your resource group to check if the API Connections are being used or not. The goal of this script, of course, is to identify orphaned API Connections in a single Resource Group quickly and effectively.

# About Me
**Sandro Pereira** | [DevScope](http://www.devscope.net/) | MVP & MCTS BizTalk Server 2010 | [https://blog.sandro-pereira.com/](https://blog.sandro-pereira.com/) | [@sandro_asp](https://twitter.com/sandro_asp)
