# Dependencias 

```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.10
dotnet add package Microsoft.AspNetCore.OpenApi --version 9.0.5
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.10
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.10
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.10
dotnet add package Microsoft.NET.Test.Sdk --version 18.0.0
dotnet add package Moq --version 4.20.72
dotnet add package Newtonsoft.Json --version 13.0.4
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.4
dotnet add package Serilog --version 4.3.0
dotnet add package Serilog.AspNetCore --version 9.0.0
dotnet add package Serilog.Sinks.Console --version 6.0.0
dotnet add package Serilog.Sinks.File --version 7.0.0
dotnet add package Swashbuckle.AspNetCore --version 9.0.6
dotnet add package xunit --version 2.9.3
dotnet add package xunit.runner.visualstudio --version 3.1.5

dotnet ef migrations add MigracionFinal \
  --project src/NFLFantasyAPI.Persistence/NFLFantasyAPI.Persistence.csproj  \
  --startup-project src/NFLFantasyAPI.Presentation/NFLFantasyAPI.Presentation.csproj 

dotnet-ef database update \
    --project src/NFLFantasyAPI.Persistence/NFLFantasyAPI.Persistence.csproj \
    --startup-project src/NFLFantasyAPI.Presentation/NFLFantasyAPI.Presentation.csproj 
```
