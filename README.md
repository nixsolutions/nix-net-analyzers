# 🔍 NIX .NET Analyzers

Custom _**Roslyn Analyzers**_ which NIX team uses to enforce coding standards

## 📚 Description

By default - NIX Team uses **StyleCop Analyzers** for code quality and code style checks. The goal is to define standard approach which all projects will use.

Decision to changes those rules is up to project Tech Lead.

Applicable for: **.NET 5** and higher. Partially can work for **.NET 3.1**.

NIX Team StyleCops comes with **[NuGet Package](https://www.nuget.org/packages/NIX.Analyzers)**:  
![image](https://user-images.githubusercontent.com/119926713/208702911-39f4fe2b-b644-41b5-a21c-471bd0c63c8f.png)  
The package has a dependency on **[StyleCop.Analyzers](https://www.nuget.org/packages/StyleCop.Analyzers/)** _(1.1.118)_ so it will be automatically installed.

## ✅ Installation

* Install **[NuGet Package](https://www.nuget.org/packages/NIX.Analyzers)** to every project in solution
* If you don't have **.editorconfig** yet, the installation will automatically add the **.editorconfig** template file to the .sln folder. Or you can do it manually: you can find file in the repository by this path `nuget/NIX.Analyzers/assets/config/.editorconfig`  
  Now your project is using the rules described in the file. All warnings from .Net Analyzers (**CA**xxxx, **CS**xxxx) except **CA1058**, **CA1062** marked as Messages.  
  You can find more information about StyleCop (**SA**xxxx/**SX**xxxx) rules **[here](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/DOCUMENTATION.md)**.

## ⚙ Settings

NIX Team has few custom code rules to ensure better software quality and following clean code practices.

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
