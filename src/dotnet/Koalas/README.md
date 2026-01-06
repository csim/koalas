# Koalas 🐨

[![Build](https://github.com/csim/koalas/actions/workflows/build.yml/badge.svg)](https://github.com/csim/koalas/actions/workflows/build.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Koalas)](https://www.nuget.org/packages/Koalas/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Koalas)](https://www.nuget.org/packages/Koalas/)

**Notebook ready C# data science tools and utilities inspired by Python pandas module.**

Koalas is a .NET Standard 2.0 library that provides powerful extensions and utilities for C# development, particularly useful in Jupyter notebooks and data science scenarios. It includes string manipulation, file system helpers, text formatting builders, and more.

## 🚀 Features

### 📝 Text Building & Formatting
- **TextTableBuilder** - Create formatted tables with dynamic columns and styling
- **TextListBuilder** - Build formatted lists with customizable separators and indentation
- **TextSectionBuilder** - Create structured text sections with headers and content
- **TextFieldSetBuilder** - Format key-value pairs and field sets

### 🔧 Extension Methods
- **String Extensions** - Rich string manipulation (Before, After, Compress, Indent, etc.)
- **DirectoryInfo Extensions** - Enhanced directory operations and traversal
- **FileInfo Extensions** - Simplified file operations and metadata access
- **LINQ Extensions** - Additional LINQ operators for data processing
- **Object Extensions** - JSON serialization and utility methods

### 📁 File System Helpers
- **DirectoryInfoHelper** - Directory enumeration and ancestor traversal
- **FileInfoHelper** - File discovery and manipulation utilities

## 📦 Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Koalas
```

Or via Package Manager Console:

```powershell
Install-Package Koalas
```

## 🛠️ Usage Examples

### String Extensions

```csharp
using Koalas.Extensions;

string text = "Hello, World!";
string after = text.After("Hello, ");  // "World!"
string before = text.Before(" World");  // "Hello,"

string indented = "Line 1\nLine 2".Indent(4);
// "    Line 1\n    Line 2"
```

### Text Table Building

```csharp
using Koalas.Text;

var table = TextBuilder.Create()
    .StartTable()
    .AddColumn("Name", minWidth: 15)
    .AddColumn("Age", minWidth: 5)
    .AddColumn("City", minWidth: 10)
    .AddDataRow("John Doe", 30, "New York")
    .AddDataRow("Jane Smith", 25, "Los Angeles")
    .AddDataRow("Bob Johnson", 35, "Chicago")
    .EndTable()
    .Render();

Console.WriteLine(table);
```

### Directory Operations

```csharp
using Koalas;
using Koalas.Extensions;

var directory = new DirectoryInfo(@"C:\MyProject");

// Get all ancestor directories
var ancestors = directory.Ancestors();

// Get files with extension methods
var csharpFiles = directory.Files("*.cs");
```

### File System Helpers

```csharp
using static Koalas.DirectoryInfoHelper;
using static Koalas.FileInfoHelper;

// Static helper methods
var projectDir = Directory(@"C:\MyProject");
var sourceFiles = Files(projectDir, "*.cs");
```

### JSON Serialization

```csharp
using Koalas.Extensions;

var data = new { Name = "John", Age = 30 };
string json = data.ToJson();

// Pretty formatted JSON
string prettyJson = data.ToJson(writeIndented: true);
```


## 🎯 Target Framework

- **.NET Standard 2.0** - Compatible with .NET Framework 4.6.1+, .NET Core 2.0+, and .NET 5+
- **C# Language Version**: Latest

## 🔧 Development

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Building the Project

```bash
# Clone the repository
git clone https://github.com/csim/koalas.git
cd koalas

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Project Structure

```
src/
├── Koalas/                    # Main library
│   ├── Extensions/            # Extension methods
│   ├── Text/                  # Text building utilities
│   ├── DirectoryInfoHelper.cs # Directory operations
│   └── FileInfoHelper.cs      # File operations
├── Koalas.CommandLine/        # CLI application
└── Koalas.Tests/              # Unit tests
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by the Python pandas library
- Built for the .NET community
