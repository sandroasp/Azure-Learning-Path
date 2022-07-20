# How to get the Error Message with Logic App Try-Catch Using an Azure Function
When we are creating more enterprise processes, the business process will typically not be simple without, for example, conditions or nested conditions, cycles, switch and so on. They will be more complicated, and we usually have nested conditions, conditions inside the loops, and so on. In that case, the out-of-the-box approach will not work. 

We would expect, and we want to get the error where indeed happen. Instead, most of the times what we get is the top-level action description marked with the square in the picture, saying:
 * ActionFailed. An action failed. No dependent actions succeeded.
 
You have at least to options on the table to address and solve this issue:
 * Using a code-first approach by using an Azure Function - this is the approach we will be explaining here today.
 * And using a no-code low-code approach by using a Logic App (could be a child Logic App or global Logic App in order for you not to implement the same logic in all your workflows) - this approach we will be explaining in another post.

This sample show us how we can retrive the proper Error Message using an Azure Function 

# About Me
**Sandro Pereira** | [DevScope](http://www.devscope.net/) | MVP & MCTS BizTalk Server 2010 | [https://blog.sandro-pereira.com/](https://blog.sandro-pereira.com/) | [@sandro_asp](https://twitter.com/sandro_asp)