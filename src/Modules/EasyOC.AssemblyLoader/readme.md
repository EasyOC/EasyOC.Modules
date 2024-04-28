## Load assemblies from config file


appsettings.json
```json
"OrchardCore":{
    "ExternalLib": {
      "Paths": [ 
          "ExternalLibs", // 动态库路径 相对于App_Data 路径,或指定完整路径
          "D:\\project\\bin\\net8.0"// Your external lib 
      ]
    },
}

```
