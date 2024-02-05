# Azure Functions: Apply Intermedium XML Validations
This Azure Function allows you to perform XML validations against an XML Schema, including first-level chain support for XML Schemas. That means that it will take into consideration all the import schemas of the main XML Schema

To trigger this function, you need to:
- In the **Body**, the XML payload that you want to be validated.
- You should specigy the following mandatory headers:
   - **Content-Type** as text/xml (or application/xml).
   - **SchemaFileName** with the name of the XML Schema (XSD) file present in the storage account.

# About US
**Sandro Pereira** | [DevScope](http://www.devscope.net/) | MVP & MCTS BizTalk Server 2010 | [https://blog.sandro-pereira.com/](https://blog.sandro-pereira.com/) | [@sandro_asp](https://twitter.com/sandro_asp)

**Luis Rigueira** | [DevScope](http://www.devscope.net/) | Enterprise Integration Consultant | [@LuisRigueira](https://twitter.com/LuisRigueira)