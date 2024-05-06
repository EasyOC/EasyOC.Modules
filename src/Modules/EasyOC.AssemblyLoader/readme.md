## Load assemblies from config file


appsettings.json
```json
"OrchardCore":{
    "ExternalLib": {
      "Paths": [ 
          "ExternalLibs", // Dynamic library path Relative to App_Data path, or specify the full path.
          "D:\\project\\bin\\net8.0"// Your external lib 
      ]
    },
}

```
