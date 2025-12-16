# GitHub Actions Workflows for Windows Desktop Script UI

This repository now includes automated GitHub Actions workflows for building and publishing your WinUI 3 application as single-file executables.

## ?? Available Workflows

### 1. **Build and Publish** (`build-and-publish.yml`)
**Triggers:** 
- Push to `main`, `master`, or `develop` branches
- Pull requests to `main` or `master`
- Manual dispatch with optional release creation
- GitHub releases

**What it does:**
- Builds for **x64**, **x86**, and **ARM64** platforms
- Creates single-file executables for each platform
- Uploads build artifacts
- Automatically attaches executables to GitHub releases

### 2. **Quick Release** (`quick-release.yml`)
**Triggers:**
- Manual dispatch only

**What it does:**
- Quick build for x64 platform only
- Creates immediate GitHub release
- Perfect for rapid releases

## ?? How to Use

### Option 1: Automatic Build on Push
1. Simply push your code to `main`, `master`, or `develop`
2. GitHub Actions will automatically build and create artifacts
3. Download artifacts from the Actions tab

### Option 2: Create a Release
1. Go to **Actions** ? **Quick Release**
2. Click **Run workflow**
3. Enter version (e.g., `v1.2.3`)
4. Choose if it's a pre-release
5. Click **Run workflow**
6. Your release will be created with the executable attached!

### Option 3: Manual Build with Release
1. Go to **Actions** ? **Build and Publish**
2. Click **Run workflow**
3. Check "Create GitHub Release" if desired
4. Click **Run workflow**

## ?? What You Get

### Build Artifacts
Each successful build creates artifacts containing:
- `Windows Desktop Script UI.exe` - Single-file executable (~66MB)
- `Windows Desktop Script UI.pdb` - Debug symbols

### Release Assets
When creating releases, you get:
- **x64**: `WindowsDesktopScriptUI-v1.0.0-win-x64.exe`
- **x86**: `WindowsDesriptScriptUI-v1.0.0-win-x86.exe` 
- **ARM64**: `WindowsDesktopScriptUI-v1.0.0-win-ARM64.exe`

## ?? Workflow Features

### ? **Automated Features**
- **Single-file publishing** with all dependencies included
- **Multi-platform builds** (x64, x86, ARM64)
- **Automatic versioning** with timestamps
- **Artifact retention** (90 days)
- **Release automation** with detailed descriptions
- **File size reporting** in build logs

### ?? **Configuration**
The workflows use these publish settings:
```xml
PublishSingleFile=true
IncludeNativeLibrariesForSelfExtract=true
IncludeAllContentForSelfExtract=true
WindowsAppSDKSelfContained=true
EnableCompressionInSingleFile=true
PublishTrimmed=false
SatelliteResourceLanguages=en
```

## ?? Prerequisites

### Repository Setup
1. Ensure your repository has the correct project structure
2. The workflows expect:
   - Project path: `Windows Desktop Script UI/Windows Desktop Script UI.csproj`
   - .NET 8.0 target framework
   - Windows App SDK package references

### GitHub Permissions
The workflows require:
- **Contents**: write (for creating releases)
- **Actions**: write (for uploading artifacts)

## ?? Usage Examples

### Example 1: Quick Development Release
```bash
# Push your changes
git add .
git commit -m "Add new features"
git push origin main

# Artifacts will be automatically created
# Download from GitHub Actions tab
```

### Example 2: Tagged Release
```bash
# Go to GitHub Actions ? Quick Release
# Enter: v1.0.0
# Run workflow
# Release will be created automatically
```

### Example 3: Pre-release Testing
```bash
# Go to GitHub Actions ? Quick Release  
# Enter: v1.0.0-beta
# Check "Mark as pre-release"
# Run workflow
```

## ?? Customization

### Modify Build Platforms
Edit `.github/workflows/build-and-publish.yml`:
```yaml
strategy:
  matrix:
    platform: [x64]  # Remove x86, ARM64 if not needed
```

### Change Trigger Branches
```yaml
on:
  push:
    branches: [ main, develop, your-branch ]
```

### Modify Retention Period
```yaml
- name: Upload build artifacts
  uses: actions/upload-artifact@v4
  with:
    retention-days: 30  # Change from 90 to 30 days
```

## ?? Monitoring Builds

### Check Build Status
1. Go to your repository
2. Click **Actions** tab
3. See all workflow runs and their status

### Download Artifacts
1. Click on a completed workflow run
2. Scroll to **Artifacts** section
3. Download the platform-specific build

### View Build Logs
1. Click on a workflow run
2. Click on a job (e.g., "build-and-publish (x64)")
3. Expand steps to see detailed logs

## ??? Troubleshooting

### Common Issues

**Build fails with platform error:**
- Ensure you're pushing to the correct branch
- Check that the project file supports the target platform

**Missing artifacts:**
- Check build logs for errors
- Verify the executable was created in the publish step

**Release not created:**
- Ensure you have write permissions to the repository
- Check that the workflow has `GITHUB_TOKEN` permissions

### Getting Help
- Check the **Actions** tab for detailed error messages
- Review the build logs for specific error details
- Ensure all prerequisites are met

## ?? Ready to Deploy!

Your GitHub Actions are now set up! Every push will trigger a build, and you can create releases with a simple workflow dispatch. Your users can download single-file executables that run on any Windows 10/11 machine without requiring .NET installation.

**Next Steps:**
1. Push this setup to your repository
2. Go to Actions and run "Quick Release" to create your first automated release
3. Share the download links with your users!