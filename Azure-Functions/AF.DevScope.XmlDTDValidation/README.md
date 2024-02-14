# Azure Functions: Validate XML against DTD
This Azure Function allows you to perform XML validations against an DTD. DTD stands for Document Type Definition and it defines the structure and the legal elements and attributes of an XML document.

To trigger this function, you need to:
- In the **Body**, the XML payload that you want to be validated.
- You should specigy the following mandatory headers:
   - **Content-Type** as text/xml (or application/xml).
   - **DTDFileName** with the name of the DTD file present in the storage account.

# About US
**Sandro Pereira** | [DevScope](http://www.devscope.net/) | MVP & MCTS BizTalk Server 2010 | [https://blog.sandro-pereira.com/](https://blog.sandro-pereira.com/) | [@sandro_asp](https://twitter.com/sandro_asp)