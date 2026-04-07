# Development Guide

Instructions for building, testing, and contributing to DeleteOld.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later
- [Git](https://git-scm.com/)
- Text editor or IDE (VS Code, Visual Studio, JetBrains Rider)
- **Recommended**: [VS Code](https://code.visualstudio.com/) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

## Project Structure

```
deleteold/
├── README.md                           # User documentation
├── DEVELOPMENT.md                      # This file
├── LICENSE                             # MIT License
│
├── .github/
│   └── workflows/
│       └── release.yml                 # CI/CD pipeline
│
├── CodeBureau.DeleteOld/               # Main application
│   ├── CodeBureau.DeleteOld.csproj
│   ├── Program.cs                      # CLI entry point
│   ├── DeleteOldService.cs             # Core deletion logic
│   └── DeleteOldOptions.cs             # Configuration DTO
│
├── CodeBureau.DeleteOld.Tests/         # Unit tests
│   ├── CodeBureau.DeleteOld.Tests.csproj
│   └── DeleteOldServiceTests.cs        # 13 xUnit tests
│
├── build/                              # Build artifacts (gitignored)
├── .gitignore                          # Standard .NET gitignore
└── Codebureau.DeleteOld.sln            # Visual Studio solution
```

## Quick Start

### 1. Clone and Build

```bash
git clone https://github.com/[your-repo]/deleteold.git
cd deleteold

# Build
dotnet build

# Run tests
dotnet test

# Run the app
dotnet run -- --help
```

### 2. Run Locally

```bash
# Show help
dotnet run -- --help

# Try a simulation (doesn't delete anything)
dotnet run -- --path . --age 365 --timeframe d --simulate

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### 3. Publish for Distribution

```bash
# Build self-contained windows executable
dotnet publish -c Release -o ./dist/windows -r win-x64 --self-contained

# Build self-contained macOS executable
dotnet publish -c Release -o ./dist/macos -r osx-x64 --self-contained

# Build self-contained linux executable
dotnet publish -c Release -o ./dist/linux -r linux-x64 --self-contained
```

The executables will be in `dist/[platform]/DeleteOld(.exe)`.

## Development Workflow

### Making Changes

1. **Create a branch** for your feature/fix:
   ```bash
   git checkout -b feature/my-feature
   ```

2. **Make your changes** in the appropriate file:
   - Core logic: `CodeBureau.DeleteOld/DeleteOldService.cs`
   - Options/configuration: `CodeBureau.DeleteOld/DeleteOldOptions.cs`
   - CLI handling: `CodeBureau.DeleteOld/Program.cs`
   - Tests: `CodeBureau.DeleteOld.Tests/DeleteOldServiceTests.cs`

3. **Add or update tests** to cover your changes:
   ```bash
   # Add a test method to DeleteOldServiceTests.cs
   [Fact]
   public void MyNewFeature_DoesExpectedBehavior()
   {
       // Arrange
       var service = new DeleteOldService();
       
       // Act
       var result = service.Execute(options);
       
       // Assert
       Assert.Equal(0, result);
   }
   ```

4. **Build and validate**:
   ```bash
   dotnet build
   dotnet test
   ```

5. **Commit with clear messages**:
   ```bash
   git commit -m "feat: add option to preserve recent files

   - Implements --keep-count flag to preserve N most recent files
   - Adds DeleteOldService.GetRecentFiles() method
   - Adds 2 new tests for preservation logic
   - Updates README with usage example"
   ```

6. **Push and create a pull request**:
   ```bash
   git push origin feature/my-feature
   ```

### Code Style

- **C# conventions**: Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Naming**: 
  - Classes/Methods: PascalCase
  - Properties: PascalCase
  - Private fields: camelCase with `_` prefix
  - Local variables: camelCase
- **Comments**: Use `///` for public APIs; `//` for implementation notes
- **Async**: Not required now, but use `async/await` if added later

### Adding Dependencies

```bash
# Add a NuGet package
dotnet add CodeBureau.DeleteOld package SomePackage --version 1.2.3

# Remove a package
dotnet remove CodeBureau.DeleteOld package SomePackage
```

**Note**: Keep dependencies minimal. Discuss large additions in an issue first.

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test

```bash
dotnet test --filter "DeleteOldServiceTests.Execute_DeletesAllFiles"
```

### Run with Coverage (requires tool)

```bash
dotnet tool install -g coverlet.console
cd CodeBureau.DeleteOld.Tests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

### Test Structure

Tests use **xUnit** with an `IDisposable` pattern for setup/cleanup:

```csharp
public class DeleteOldServiceTests : IDisposable
{
    private readonly string _testPath;

    public DeleteOldServiceTests()
    {
        // Setup: Create temp directory
        _testPath = Path.Combine(Path.GetTempPath(), "DeleteOldTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testPath);
    }

    public void Dispose()
    {
        // Cleanup: Remove temp directory
        if (Directory.Exists(_testPath))
            Directory.Delete(_testPath, true);
    }

    [Fact]
    public void TestName_ExpectedBehavior_Outcome()
    {
        // Arrange
        var options = new DeleteOldOptions { Path = _testPath, Age = 10 };
        var service = new DeleteOldService(output: null);

        // Act
        var result = service.Execute(options);

        // Assert
        Assert.Equal(0, result);
    }
}
```

## Debugging

### In VS Code

1. Install [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
2. Set breakpoints by clicking in the gutter
3. Press `F5` to start debugging
4. Step through code with `F10` (over) / `F11` (into)

### In Visual Studio

1. File → Open Project → `Codebureau.DeleteOld.sln`
2. Set breakpoints and press `F5` to debug
3. Use Debug menu for step over/into/out

### From Command Line

```bash
# Run tests with diagnostic output
dotnet test --logger "console;verbosity=diag"

# Run with verbose build output
dotnet build --verbosity diagnostic
```

## Updating Version

When releasing a new version:

1. **Update version in `.csproj`**:
   ```xml
   <!-- In CodeBureau.DeleteOld/CodeBureau.DeleteOld.csproj -->
   <PropertyGroup>
       <Version>2.0.1</Version>
       <AssemblyVersion>2.0.1.0</AssemblyVersion>
       <FileVersion>2.0.1.0</FileVersion>
   </PropertyGroup>
   ```

2. **Update README.md** — Add entry to Changelog section:
   ```markdown
   ### v2.0.1
   - 🐛 Fixed edge case with empty directory recursion
   - 📝 Improved documentation
   ```

3. **Commit**:
   ```bash
   git commit -m "chore: bump version to 2.0.1"
   ```

4. **Tag the commit**:
   ```bash
   git tag -a v2.0.1 -m "Release version 2.0.1"
   git push origin v2.0.1
   ```

   This triggers the GitHub Actions CI/CD pipeline to build and release.

## CI/CD Pipeline (.github/workflows/release.yml)

When you push a version tag (e.g., `v2.0.1`), GitHub Actions automatically:

1. ✅ Runs all tests
2. 🔨 Builds for Windows (x64)
3. 🔨 Builds for macOS (x64)
4. 🔨 Builds for Linux (x64)
5. 📦 Creates GitHub Release with executables
6. 📦 Publishes to Chocolatey (requires API key in secrets)

See `.github/workflows/release.yml` for the full pipeline definition.

## Chocolatey Packaging

The Chocolatey package is built from:

```
choco/
├── tools/
│   ├── chocolateyinstall.ps1
│   ├── chocolateyuninstall.ps1
│   └── DeleteOld.exe (added during release)
└── deleteold.nuspec
```

**Manual Chocolatey release**:

```bash
# Create package
choco pack choco/deleteold.nuspec

# Push to Chocolatey (requires API key)
choco push deleteold.2.0.1.nupkg --key [your-api-key]
```

## Troubleshooting

### Build fails: "SDK not found"
```bash
# Install .NET 9 SDK from https://dotnet.microsoft.com/download
dotnet --version
```

### Tests fail: "Permission denied on temp files"
Windows: Ensure no explorer windows are open to temp folders.  
Linux/macOS: Check file permissions — may need elevated privileges.

### CI/CD doesn't trigger
- Ensure tag format is `vX.Y.Z` (e.g., `v2.0.1`)
- Check Actions tab for logs: https://github.com/[user]/deleteold/actions

## Resources

- **[.NET Documentation](https://docs.microsoft.com/dotnet)** — Comprehensive guide
- **[xUnit Testing](https://xunit.net/)** — Testing framework docs
- **[GitHub Actions](https://docs.github.com/actions)** — CI/CD platform

## Getting Help

- 💬 Open a [Discussion](https://github.com/[user]/deleteold/discussions) for questions
- 🐛 Report bugs in [Issues](https://github.com/[user]/deleteold/issues)
- 💡 Suggest features in [Issues](https://github.com/[user]/deleteold/issues)

## Release Checklist

Before releasing a new version:

- [ ] All tests pass (`dotnet test`)
- [ ] Code builds without warnings (`dotnet build`)
- [ ] DEVELOPMENT.md is up-to-date
- [ ] README.md Changelog is updated
- [ ] Version number updated in `.csproj`
- [ ] Commit and push changes
- [ ] Create Git tag: `git tag -a vX.Y.Z -m "Release message"`
- [ ] Push tag: `git push origin vX.Y.Z`
- [ ] Verify CI/CD pipeline completes
- [ ] GitHub Release is created automatically
- [ ] Chocolatey package is published automatically (if configured)

---

**Thank you for contributing!** 🎉
