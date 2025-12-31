# Build Instructions

## Prerequisites

1. **.NET 8.0 SDK** - Download from [Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **TSP_CS.Core.dll** - The core algorithm library (see below)

## Obtaining the Core DLL

The `TSP_CS.Core.dll` file contains the proprietary algorithm implementation. You have several options:

### Option 1: DLL Included in Repository (Recommended)
- The DLL should be in the `libs/` folder
- If present, you can build immediately
- If missing, contact the repository maintainer

### Option 2: Build from Private Repository
If you have access to the private `TSP_CS-Core` repository:

1. Clone the private repository
2. Build the Core project:
   ```bash
   cd TSP_CS-Core
   dotnet build TSP_CS.Core.csproj -c Release
   ```
3. Copy the DLL to this repository:
   ```bash
   copy bin\Release\net8.0-windows\TSP_CS.Core.dll ..\TSP_HSM-App\libs\
   ```

### Option 3: Download Separately
- Contact the maintainer for a download link
- Place the DLL in `libs/TSP_CS.Core.dll`

## Building the Application

### Using Visual Studio

1. Open `TSP_HSM-App.csproj` in Visual Studio 2022
2. Ensure `TSP_CS.Core.dll` exists in `libs/` folder
3. Build → Build Solution (Ctrl+Shift+B)
4. Run → Start Debugging (F5)

### Using Command Line

```bash
# Navigate to project directory
cd TSP_HSM-App

# Verify DLL exists
dir libs\TSP_CS.Core.dll

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Run the application
dotnet run
```

## Project Dependencies

The project file (`TSP_HSM-App.csproj`) references:
- `TSP_CS.Core.dll` from `libs/` folder
- .NET 8.0 Windows Forms framework

## Troubleshooting

### Error: "Could not find TSP_CS.Core.dll"

**Solution:** Ensure the DLL is in the `libs/` folder relative to the project file.

### Error: "FileNotFoundException: Could not load file or assembly 'TSP_CS.Core'"

**Solution:** 
- Verify the DLL is built for .NET 8.0
- Check that the DLL is in the correct location
- Ensure all dependencies of the Core DLL are available

### Error: "The type or namespace name 'TspMain' could not be found"

**Solution:** 
- The `TspMain` class is in `TSP_CS.Core.dll`
- Verify the DLL reference in the project file
- Rebuild the solution

## File Structure After Build

```
TSP_HSM-App/
├── bin/
│   └── Debug/
│       └── net8.0-windows/
│           ├── TSP_HSM-App.exe
│           ├── TSP_HSM-App.dll
│           ├── TSP_CS.Core.dll      # Copied from libs/
│           └── [other dependencies]
├── libs/
│   └── TSP_CS.Core.dll              # Source DLL
└── [source files]
```

## Notes

- The Core DLL must match the .NET version (8.0)
- The DLL is copied to the output directory during build
- All public types from `TSP_CS.Core` namespace are accessible to the application

