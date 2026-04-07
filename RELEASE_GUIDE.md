# Release Guide

Step-by-step instructions for releasing a new version of DeleteOld.

## Before You Release

1. ✅ **Ensure all tests pass**:
   ```bash
   dotnet test
   ```

2. ✅ **Ensure code builds without warnings**:
   ```bash
   dotnet build --configuration Release
   ```

3. ✅ **Review recent commits**:
   ```bash
   git log --oneline -10
   ```

4. ✅ **Update version in `.csproj`**:
   Edit `CodeBureau.DeleteOld/CodeBureau.DeleteOld.csproj`:
   ```xml
   <PropertyGroup>
       <Version>2.0.1</Version>
       <AssemblyVersion>2.0.1.0</AssemblyVersion>
       <FileVersion>2.0.1.0</FileVersion>
   </PropertyGroup>
   ```

5. ✅ **Update README.md Changelog**:
   Add entry at the top of the Changelog section:
   ```markdown
   ### v2.0.1
   - 🐛 Fixed edge case with empty directory recursion
   - 📝 Improved documentation
   ```

6. ✅ **Update DEVELOPMENT.md** if needed (e.g., new dependencies)

## Release Steps

### Step 1: Commit Version Bump

```bash
git add CodeBureau.DeleteOld/CodeBureau.DeleteOld.csproj README.md DEVELOPMENT.md
git commit -m "chore: bump version to 2.0.1

- Updated version in .csproj
- Updated README Changelog
- Documented changes"
```

### Step 2: Create and Push Git Tag

```bash
# Create annotated tag with release notes
git tag -a v2.0.1 -m "Release version 2.0.1

Features:
- Fixed edge case with empty directory recursion
- Improved documentation

See README.md for full changelog."

# Push the tag to GitHub (this triggers CI/CD)
git push origin v2.0.1
```

**Important**: The tag format MUST be `vX.Y.Z` (e.g., `v2.0.1`) for the CI/CD workflow to trigger.

### Step 3: Monitor CI/CD Pipeline

GitHub Actions will automatically:
1. ✅ Run tests on Windows, macOS, and Linux
2. ✅ Build self-contained executables for all platforms
3. ✅ Create a GitHub Release with downloadable assets
4. ✅ Publish to Chocolatey (if API key is configured)

Monitor progress at: `https://github.com/[your-repo]/deleteold/actions`

### Step 4: Verify Release

Once the pipeline completes:

1. **Check GitHub Release**: https://github.com/[your-repo]/deleteold/releases/tag/v2.0.1
   - Should have downloadable executables:
     - `deleteold-win-x64.zip`
     - `deleteold-linux-x64.tar.gz`
     - `deleteold-osx-x64.tar.gz`

2. **Test downloaded executable**:
   ```bash
   # Extract the zip/tar.gz
   # Run the executable
   ./DeleteOld --help
   ```

3. **Check Chocolatey** (if configured):
   ```bash
   choco info deleteold
   ```

## Post-Release

### Announce the Release

1. **GitHub Release Notes**: Already automatic from README Changelog

2. **Social Media** (optional):
   - Tweet about the release
   - Post in relevant communities

3. **Update Documentation** (optional):
   - Update any external wikis
   - Notify users if breaking changes

## Troubleshooting

### CI/CD Pipeline Failed

**Check the logs**:
- Go to https://github.com/[your-repo]/deleteold/actions
- Click the failed workflow
- Review individual step logs

**Common issues**:
- **Test failure**: Run `dotnet test` locally to debug
- **Build failure**: Run `dotnet build` locally
- **Chocolatey failure**: Ensure `CHOCOLATEY_API_KEY` secret is set

### Release Doesn't Show Up

1. **Tag format wrong**: Must be `vX.Y.Z` (with `v` prefix)
   ```bash
   # Delete and recreate the tag
   git tag -d v2.0.1
   git push origin :v2.0.1
   git tag -a v2.0.1 -m "Release v2.0.1"
   git push origin v2.0.1
   ```

2. **Push didn't reach GitHub**: Verify with:
   ```bash
   git ls-remote origin | grep tags
   ```

3. **Workflow didn't trigger**: Check if tag matches pattern in `.github/workflows/release.yml`

### Manual Release (Backup)

If automated release fails, you can build and release manually:

```bash
# Build for all platforms
dotnet publish -c Release -r win-x64 --self-contained -o ./publish/windows
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish/linux
dotnet publish -c Release -r osx-x64 --self-contained -o ./publish/macos

# Create archives
cd publish/windows && 7z a -tzip ../../deleteold-win-x64.zip * && cd ../..
cd publish/linux && tar -czf ../../deleteold-linux-x64.tar.gz * && cd ../..
cd publish/macos && tar -czf ../../deleteold-osx-x64.tar.gz * && cd ../..

# Use GitHub web UI to create Release and upload these archives manually
```

## Scheduled Maintenance Release

For routine updates without new features:

1. **Format**: `v2.0.1+patch` (add `.patch` number)
2. **Wait time**: Can release frequently if tests pass
3. **Changelog**: Keep it brief for maintenance releases

## Major Version Release

For significant updates (e.g., `v3.0.0`):

1. **Breaking changes**: Document in README prominently
2. **Migration guide**: Add section to DEVELOPMENT.md if needed
3. **Backward compatibility**: Note in release notes
4. **Lead time**: Consider announcing in advance

Example tag:
```bash
git tag -a v3.0.0 -m "Major release: Complete rewrite

BREAKING CHANGES:
- Old CLI format no longer supported
- Configuration file format changed

See README.md for migration guide."
```

## Version Numbering (Semantic Versioning)

Follow [Semantic Versioning](https://semver.org/):

- **Major** (v2→v3): Breaking changes (API incompatibility)
- **Minor** (v2.0→v2.1): New features (backward compatible)
- **Patch** (v2.0.0→v2.0.1): Bug fixes (backward compatible)

Examples:
- `v1.0.0` — Initial release
- `v1.1.0` — Added new feature (--keep-count flag)
- `v1.1.1` — Fixed bug in recursive mode
- `v2.0.0` — Modernized to .NET 9 (breaking changes)

## Rollback

If a bad release goes out:

```bash
# Option 1: Delete the tag and release
git tag -d v2.0.1
git push origin :v2.0.1
# Go to GitHub and delete the Release manually

# Option 2: Create a quick patch release
# (Recommended if users already downloaded)
# Apply fixes, bump to v2.0.2, release normally
```

Then announce the issue and recommend updating to the fixed version.

---

**Questions?** See DEVELOPMENT.md for contributor info, or open a GitHub Issue.
