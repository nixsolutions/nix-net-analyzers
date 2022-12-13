# 🔍 NIX .NET StyleCop Analyzers

Custom _**Roslyn Analyzers**_ which NIX team uses to enforce coding standards

## 📚 Description

By default - .NET Team uses **StyleCop Analyzers** for code quality and code style checks. The goal is to define standard approach which all projects will use.

Decision to changes those rules is up to project Tech Lead.

Applicable for: **.NET 5** and higher. Partially can work for **.NET 3.1**.

NIX .NET Department StyleCops comes with **[NuGet Package](https://www.nuget.org/)**:
![nuget](https://user-images.githubusercontent.com/119926713/207299454-bc66f720-7763-4246-a2a0-65c40908ac2c.png)  
The package has a dependency on **[StyleCop.Analyzers](https://www.nuget.org/packages/StyleCop.Analyzers/)** _(1.1.118)_ so it will be automatically installed.

## ✅ Installation

* Install **[NuGet Package](https://www.nuget.org/)** to every project in solution
* If you don't have **.editorconfig** yet, the installation will automatically add the **.editorconfig** template file to the .sln folder  
  Now your project is using the rules described in the file. All warnings from .Net Analyzers (**CA**xxxx, **CS**xxxx) except **CA1058**, **CA1062** marked as Messages.  
  You can find more information about StyleCop (**SA**xxxx/**SX**xxxx) rules **[here](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/DOCUMENTATION.md)**.
* Change project  file (**.csproj**) and add next lines in <PropertyGroup> and then save the file:
  `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`  
  `<WarningsAsErrors/>`
![image](https://user-images.githubusercontent.com/119926713/207304765-e96537ec-a0b9-4f80-a3f4-c98bc20d913d.png)
This will prevent project building in case of style errors.

## ⚙ Settings

NIX .NET Department has few custom code rules to ensure better software quality and following clean code practices.

Custom rules:
1. Line must not be longer than **200** characters;
2. Method must not be larger than **50** lines;
3. File must not be larger than **1000** lines;
4. Method must not have more than **5** parameters;
5. Constructor must not have more than **5** parameters;

To override these rules you must use **.editorconfig** and specify which rule you want to override:
`dotnet_diagnostic.Nix01.max_line_length = 200`  
`dotnet_diagnostic.Nix02.max_method_lines = 50`  
`dotnet_diagnostic.Nix03.max_file_lines = 1000`  
`dotnet_diagnostic.Nix04.max_params_in_method = 5`  
`dotnet_diagnostic.Nix05.max_params_in_ctor = 5`
