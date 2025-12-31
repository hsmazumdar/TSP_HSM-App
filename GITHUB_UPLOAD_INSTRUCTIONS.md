# GitHub Upload Instructions

## Step 1: Create Repository on GitHub

1. **Go to GitHub**: Open https://github.com/hsmazumdar in your browser
2. **Click "New"** (or the "+" icon in the top right, then "New repository")
3. **Repository Settings**:
   - **Repository name**: `TSP_HSM-App`
   - **Description**: `TSP Solver Application - Public WinForms UI with proprietary Core algorithms`
   - **Visibility**: Select **Public** (or Private if you prefer)
   - **DO NOT** check any of these boxes:
     - ❌ Add a README file
     - ❌ Add .gitignore
     - ❌ Choose a license
   - (We already have these files locally)
4. **Click "Create repository"**

## Step 2: Push Local Repository

After creating the repository on GitHub, run these commands in PowerShell:

```powershell
cd D:\_December2025\_TSP31122025\GitHub\TSP_HSM-App
git push -u origin main
```

If you're prompted for credentials:
- **Username**: `hsmazumdar`
- **Password**: Use a **Personal Access Token** (not your GitHub password)
  - To create a token: https://github.com/settings/tokens
  - Select "repo" scope
  - Copy the token and use it as the password

## Alternative: Use SSH (if configured)

If you have SSH keys set up with GitHub, you can use:

```powershell
cd D:\_December2025\_TSP31122025\GitHub\TSP_HSM-App
git remote set-url origin git@github.com:hsmazumdar/TSP_HSM-App.git
git push -u origin main
```

## What Will Be Uploaded

✅ **Included:**
- All UI source code (C# files)
- Designer and resource files
- Project file (.csproj)
- **Core DLL binary** (`libs/TSP_CS.Core.dll`)
- Documentation (README.md, etc.)
- Analysis scripts

❌ **Excluded (as requested):**
- Core source code (TspMain.cs, TspDistanceMatrix.cs)
- Build artifacts (bin/, obj/)
- Cache files

## Verification

After pushing, verify at:
https://github.com/hsmazumdar/TSP_HSM-App

You should see:
- All source files
- `libs/TSP_CS.Core.dll` (binary, not source)
- README.md
- Documentation files

