# References from Julie's Design Models
You can take the methods of Classes.docx and format them to C#(ish) by using these regexes:
```
^(\t\+ )(.*) // Find
public $2 {\n\t\n}\n // Replace
```