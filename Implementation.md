# Implementation

1. Backend Setup (.NET 8.0 Web API)

To initialize a new **.NET 8.0 Web API** project, follow these steps:

---

### ✅ Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed
* Terminal/Command Prompt or Visual Studio

---

## 🔧 Using CLI (Command Line Interface)

### 1. **Open Terminal or Command Prompt**

Navigate to the directory where you want to create your project.

### 2. **Run the following command**

```bash
dotnet new webapi -n AuthProjectAPI --framework net8.0
```

* `webapi` – template for Web API
* `-n AuthProjectAPI` – project name
* `--framework net8.0` – explicitly target .NET 8.0

### 3. **Navigate into the project folder**

```bash
cd AuthProjectAPI
```

### 4. **Run the project**

```bash
dotnet run
```

You should see output like:

```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
```

Open a browser and visit:
`https://localhost:5001/weatherforecast`

---

## 📁 Project Structure Overview

| Folder/File           | Description                                  |
| --------------------- | -------------------------------------------- |
| `Controllers/`        | Contains API controllers                     |
| `Program.cs`          | Minimal hosting and middleware configuration |
| `appsettings.json`    | Configuration settings                       |
| `AuthProjectAPI.csproj` | Project configuration file                   |

---

## 💡 Optional Flags

You can customize the project with flags like:

* `--no-https` – disables HTTPS
* `--use-minimal-apis` – uses .NET minimal APIs instead of controllers
* `--auth` – add authentication (e.g., `--auth Individual`)

---

### Add Required NuGet Packages

`dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.5`
`dotnet add package Swashbuckle.AspNetCore --version 6.5.0`
`dotnet add package System.IdentityModel.Tokens.Jwt`
`dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.5`
`dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.5`
`dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.5`

---

### Run Entity Framework Core migration to create tables for User Authentication

Install if not already done: `dotnet tool install --global dotnet-ef `
`dotnet ef migrations add InitialIdentitySchema`
`dotnet ef database update`
