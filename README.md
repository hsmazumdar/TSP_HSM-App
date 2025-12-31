# TSP Solver Application - Public Repository

This is the public-facing application shell for the Traveling Salesman Problem (TSP) Solver. The core algorithmic implementation is provided as a binary DLL (`TSP_CS.Core.dll`) to protect intellectual property.

## Overview

High-performance TSP solver supporting up to 1,000,000 cities with:
- k-nearest neighbors optimization
- Adaptive virtual distance learning
- Multiple city distribution patterns (Random, Square, Circle, Spiral)
- Real-time visualization
- Configurable algorithm parameters

## Architecture

This repository contains:
- **Application Shell** (Public): Windows Forms UI, dialogs, and user interface code
- **Core Algorithms** (Private): Provided as `TSP_CS.Core.dll` in the `libs/` folder

### Project Structure

```
TSP_HSM-App/
├── Program.cs                    # Application entry point
├── Tsp.cs                        # Main Windows Forms UI
├── TspTune.cs                    # Settings/Parameters dialog
├── MatrixChoiceDialog.cs         # Cache selection dialog
├── *.Designer.cs                 # Windows Forms designer files
├── *.resx                        # Resource files
├── libs/                         # Pre-built DLL location
│   └── TSP_CS.Core.dll          # Core algorithm library (binary)
├── TSP_HSM-App.csproj           # Project file
└── README.md                     # This file
```

## Requirements

- .NET 8.0 SDK or later
- Windows (Windows Forms application)
- Visual Studio 2022 or later (recommended)

## Building the Application

1. **Ensure the Core DLL is present:**
   - The `TSP_CS.Core.dll` file must be in the `libs/` folder
   - If missing, see `build-instructions.md` for details

2. **Build the project:**
   ```bash
   dotnet build TSP_HSM-App.csproj
   ```

3. **Run the application:**
   ```bash
   dotnet run --project TSP_HSM-App.csproj
   ```

Or open the project in Visual Studio and press F5.

## Usage

1. **Configure Parameters:**
   - Click the "TUNE" button to open the settings dialog
   - Adjust algorithm parameters (thresholds, normalization, etc.)
   - Set city count and distribution pattern

2. **Generate Cities:**
   - Select city distribution pattern (Random/Square/Circle/Spiral)
   - Enter number of cities
   - Click "DRAW" to generate cities

3. **Solve TSP:**
   - Click "TOUR" to start the algorithm
   - Watch progress in real-time (if Display checkbox is enabled)
   - Click "STOP" to halt execution
   - Navigate through trial solutions using the Trial spinner

## Core Algorithm Library

The core algorithms (`TSP_CS.Core.dll`) are proprietary and provided as a binary reference. This includes:
- TSP solving algorithms (Toure, minima1, wrong_rout, rout_found, etc.)
- Distance matrix computation and k-NN optimization
- Template-based algorithms for square grids
- Virtual distance learning

**Note:** The source code for `TSP_CS.Core.dll` is not included in this repository. The DLL provides a public API that the application uses.

## License

[Specify your license here]

## Author

H.S. Mazumdar (Himanshu)
Original algorithm from 1993 - Physical Research Laboratory, Ahmedabad, India

## Contributing

This is a research codebase. For questions or issues, please contact the maintainer.

## Version History

- **v1.0** - Initial public release
  - Windows Forms UI
  - Core algorithm library (binary)
  - Support for up to 1M cities
  - k-NN optimization

