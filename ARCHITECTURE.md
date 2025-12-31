# Architecture Overview

## Project Split Strategy

This repository implements a two-project solution to protect intellectual property while sharing the application on GitHub.

### Public Repository (This Repository)
**TSP_HSM-App** - Application Shell
- Contains: UI code, dialogs, user interface
- Purpose: Public demonstration and usage
- License: [Your License]

### Private Repository (Not in GitHub)
**TSP_CS-Core** - Algorithm Library
- Contains: Core solver algorithms, distance matrix computation
- Purpose: Proprietary IP protection
- Distribution: Binary DLL only

## Component Breakdown

### Public Components (TSP_HSM-App)

| File | Purpose | Visibility |
|------|---------|------------|
| `Program.cs` | Application entry point | Public |
| `Tsp.cs` | Main Windows Forms UI | Public |
| `TspTune.cs` | Settings/Parameters dialog | Public |
| `MatrixChoiceDialog.cs` | Cache selection dialog | Public |
| `*.Designer.cs` | Windows Forms designer files | Public |
| `*.resx` | Resource files | Public |

### Private Components (TSP_CS.Core.dll)

| Component | Purpose | Access |
|-----------|---------|--------|
| `TspMain` | Core solver engine (Toure, minima1, wrong_rout, rout_found, etc.) | Public API |
| `TspDistanceMatrix` | k-NN optimization, template-based algorithms, distance computations | Public API |
| Internal algorithms | Proprietary implementations | Hidden |

## Public API Contract

The Core DLL exposes the following public types:

### TspMain Class
```csharp
namespace TSP_CS
{
    public class TspMain
    {
        // Algorithm parameters
        public int boundingthreshold { get; set; }
        public int minimumthreshold { get; set; }
        public int iterationfactor { get; set; }
        public int normalization { get; set; }
        public int kNeighbors { get; set; }
        public int fullScanThreshold { get; set; }
        
        // Visualization parameters
        public int NodeRadiusPixels { get; set; }
        public int MarginPixels { get; set; }
        
        // Algorithm state
        public int trials { get; set; }
        public int rout { get; set; }
        public int minrout { get; set; }
        public int numcity { get; set; }
        
        // Methods
        public void PopulateCityes(int ports, int pattern);
        public void Restart();
        public void StartTour();
        public void StartAlgorithmThread();
        public void StopAlgorithmThread();
        // ... other public methods
    }
}
```

### TspDistanceMatrix Class
```csharp
namespace TSP_CS
{
    public class TspDistanceMatrix
    {
        public int GetDistance(int i, int j);
        public int GetVirtualDistance(int i, int j);
        public void BuildMatrices(int pattern, bool skipCacheLoad = false);
        // ... other public methods
    }
}
```

## Dependency Flow

```
TSP_HSM-App (Public)
    │
    ├── References: TSP_CS.Core.dll (Binary)
    │
    └── Uses: TspMain, TspDistanceMatrix (Public API)
```

## Build Process

1. **Core DLL Build** (Private Repository)
   - Build `TSP_CS.Core` project
   - Output: `TSP_CS.Core.dll`
   - Copy to `TSP_HSM-App/libs/`

2. **Application Build** (This Repository)
   - Reference `libs/TSP_CS.Core.dll`
   - Build application
   - DLL is copied to output directory

## IP Protection Strategy

### What's Protected
- ✅ Core algorithm implementations
- ✅ Distance matrix computation logic
- ✅ k-NN optimization details
- ✅ Template-based algorithms
- ✅ Virtual distance learning

### What's Public
- ✅ User interface code
- ✅ Dialog implementations
- ✅ Application structure
- ✅ Public API usage examples

### Protection Level
- **Source Code**: Not visible (DLL only)
- **Reverse Engineering**: Possible but difficult (obfuscation can be added)
- **API Surface**: Public and documented
- **Algorithm Details**: Hidden in binary

## Future Enhancements

### Option 1: Interface-Based Design
Create interfaces to further abstract the Core DLL:
```csharp
public interface ITspSolver
{
    void Solve(int[] cities);
    int GetBestDistance();
}
```

### Option 2: Obfuscation
Apply code obfuscation to the Core DLL for additional protection.

### Option 3: NuGet Package
Distribute Core DLL as a NuGet package from a private feed.

## Notes

- The current implementation uses direct class instantiation (`new TspMain()`)
- This works because `TspMain` is a public class in the Core DLL
- For better abstraction, consider implementing interfaces in a future version
- The DLL must be present in `libs/` for the application to build

