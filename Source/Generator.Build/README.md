# Protocol Generator MSBuild Integration

This package provides MSBuild integration for the Cratis Protocol Generator, enabling automatic generation of gRPC service interfaces from your .NET assemblies during the build process.

## Installation

Add the package reference to your project:

```xml
<PackageReference Include="Cratis.ProtocolGeneration.Generator.Build" Version="*" />
```

## Usage

The generator runs automatically before compilation. By default, it will:
- Generate protocol interfaces in `$(ProjectDir)../Interfaces/Generated`
- Use `Interfaces` as the base namespace
- Skip 1 namespace segment from the assembly name

## Configuration

Customize the generator behavior by setting properties in your project file:

```xml
<PropertyGroup>
    <!-- Enable/disable the generator (default: true) -->
    <ProtocolGenerator_Enabled>true</ProtocolGenerator_Enabled>
    
    <!-- Number of namespace segments to skip (default: 1) -->
    <ProtocolGenerator_SkipSegments>1</ProtocolGenerator_SkipSegments>
    
    <!-- Base namespace for generated files (default: Interfaces) -->
    <ProtocolGenerator_BaseNamespace>Interfaces</ProtocolGenerator_BaseNamespace>
    
    <!-- Output path for generated files (default: $(ProjectDir)../Interfaces/Generated) -->
    <ProtocolGenerator_OutputPath>$(ProjectDir)../Interfaces/Generated</ProtocolGenerator_OutputPath>
</PropertyGroup>
```

## Example

Given a service in assembly `MyCompany.MyProduct.Backend`:

```csharp
namespace MyCompany.MyProduct.Backend.Authors;

[GenerateInterface]
public class AuthorCommands
{
    public Task CreateAuthor(AuthorId id, string name) { ... }
}
```

With default settings (`SkipSegments=1`, `BaseNamespace=Interfaces`), generates:

```
Interfaces/Generated/
└── MyProduct.Backend.Authors/
    └── IAuthorCommands.cs
```

## Disable Generation

To disable generation for a specific project:

```xml
<PropertyGroup>
    <ProtocolGenerator_Enabled>false</ProtocolGenerator_Enabled>
</PropertyGroup>
```
