# Azure Functions: Apply XSLT Transformation
This Azure Function converts a XML payload into another XML, JSON or any other format using XLST. To use this function you need to setup an Azure Storage Account and a container to store the XSLT files.

To trigger this function you need to:
 - In the **Body** send a XML payload
 - You should specigy the following mandatory headers:
   - **Content-Type** as text/xml or application/xml
   - **XsltFileName** with the name of the xslt file present in the storage account.
 - Optionally you can set the following header:
   - **Output-Content-Type**: this will specify the outcome (response) content-type. Default value is text/xml

# About US
**Sandro Pereira** | [DevScope](http://www.devscope.net/) | MVP & MCTS BizTalk Server 2010 | [https://blog.sandro-pereira.com/](https://blog.sandro-pereira.com/) | [@sandro_asp](https://twitter.com/sandro_asp)

**Luis Rigueira** | [DevScope](http://www.devscope.net/) | Enterprise Integration Consultant | [@LuisRigueira](https://twitter.com/LuisRigueira)