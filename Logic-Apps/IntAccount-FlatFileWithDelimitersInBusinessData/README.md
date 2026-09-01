## POC: Logic Apps - Processing Flat Files With Delimiters inside business data

# Introduction
CSV files frequently contain field delimiters as part of the business data, such as commas inside addresses or product descriptions. While many developers resort to custom code or preprocessing techniques to solve this problem, BizTalk Server and Logic Apps already include a built-in capability designed for this scenario. In this article, we show how to create a Flat File Schema and configure the Wrap Character property to correctly parse delimited files with quoted values, providing a clean, maintainable, code-free solution.

# About Me
**Sandro Pereira** | [DevScope](http://www.devscope.net/) | MVP & MCTS BizTalk Server 2010 | [https://blog.sandro-pereira.com/](https://blog.sandro-pereira.com/) | [@sandro_asp](https://twitter.com/sandro_asp)
