#!/usr/bin/env python3
"""
Analyze TSP Solution Quality for N=100,000
Analyzes rout_seq_*.tsp files and calculates gap percentages
"""

import os
import glob
from pathlib import Path
import statistics
import math

# Directory to analyze - check multiple possible locations
POSSIBLE_DIRS = [
    r"D:\_December2025\_TSP31122025\GitHub\TSP_HSM-App\bin\Release\net8.0-windows",
    r"D:\_December2025\_TSP31122025\GitHub\TSP_HSM-App\bin\Debug\net8.0-windows",
    r"D:\_December2025\_TSP31122025\GitHub\TSP_CS-NN-NK\bin\Release\net8.0-windows",
    r"D:\_December2025\_TSP31122025\GitHub\TSP_CS-NN-NK\bin\Debug\net8.0-windows",
]

def find_result_directory():
    """Find the directory containing route solution files"""
    for dir_path in POSSIBLE_DIRS:
        path = Path(dir_path)
        if path.exists():
            tsp_files = list(path.glob("rout_seq_*.tsp"))
            if tsp_files:
                # Check if any file has N=100000
                for tsp_file in tsp_files[:3]:  # Check first 3 files
                    try:
                        with open(tsp_file, 'r') as f:
                            first_line = f.readline().strip()
                            if ',' in first_line:
                                n = int(first_line.split(',')[0])
                                if n == 100000:
                                    return path
                    except:
                        pass
    return None

def parse_route_file(filepath):
    """Parse a route solution file and extract distance, solution time, and route"""
    try:
        with open(filepath, 'r') as f:
            lines = f.readlines()
        
        if len(lines) < 2:
            return None
        
        # Parse header line: size,distance,solutionTime
        header = lines[0].strip()
        if ',' in header:
            parts = header.split(',')
            size = int(parts[0])
            distance = float(parts[1])
            solution_time = float(parts[2]) if len(parts) > 2 else 0.0
        else:
            # Old format: just distance
            size = int(header)
            distance = float(lines[1].strip())
            solution_time = 0.0
        
        # Read route (city indices)
        route = []
        start_idx = 1 if ',' in header else 2
        for i in range(start_idx, min(start_idx + 1000, len(lines))):  # Read first 1000 for validation
            try:
                city_idx = int(lines[i].strip())
                route.append(city_idx)
            except ValueError:
                break
        
        # Check route completeness
        unique_cities = len(set(route)) if route else 0
        route_complete = (len(route) == size + 1) if route else False
        
        return {
            'file': os.path.basename(filepath),
            'size': size,
            'distance': distance,
            'solution_time': solution_time,
            'route_length': len(route),
            'unique_cities': unique_cities,
            'route_complete': route_complete
        }
    except Exception as e:
        print(f"Error parsing {filepath}: {e}")
        return None

def calculate_optimal_length_square_grid(n):
    """Calculate optimal length for N×N square grid with unit spacing"""
    # For a square grid with N cities:
    # If N is a perfect square: grid_size = sqrt(N)
    # Optimal tour length ≈ N (one unit per city in perfect serpentine pattern)
    # For N=100000: sqrt(100000) ≈ 316.23, so grid is approximately 316×316
    # Optimal length ≈ 100000 grid units
    grid_size = math.sqrt(n)
    optimal = n  # For unit spacing, optimal ≈ N
    return optimal, int(grid_size), int(grid_size)

def calculate_gap(solution_length, optimal_length):
    """Calculate gap percentage"""
    if optimal_length == 0:
        return None
    gap = ((solution_length - optimal_length) / optimal_length) * 100.0
    return gap

def main():
    """Main analysis function"""
    result_dir = find_result_directory()
    
    if result_dir is None:
        print("=" * 80)
        print("ERROR: Could not find route solution files for N=100,000")
        print("=" * 80)
        print("\nSearched in:")
        for dir_path in POSSIBLE_DIRS:
            print(f"  - {dir_path}")
        print("\nPlease ensure:")
        print("  1. A run with N=100,000 has been completed")
        print("  2. Route solution files (rout_seq_*.tsp) exist in the output directory")
        return
    
    # Find all route solution files
    route_files = sorted(result_dir.glob("rout_seq_*.tsp"))
    
    if not route_files:
        print(f"No route solution files found in {result_dir}")
        return
    
    print("=" * 80)
    print("TSP SOLUTION QUALITY ANALYSIS: N=100,000")
    print("=" * 80)
    print(f"Directory: {result_dir}")
    print(f"Found {len(route_files)} solution files")
    print()
    
    # Parse all files
    results = []
    for route_file in route_files:
        result = parse_route_file(route_file)
        if result:
            results.append(result)
    
    if not results:
        print("ERROR: No valid route files could be parsed")
        return
    
    # Filter for N=100000
    n100000_results = [r for r in results if r['size'] == 100000]
    
    if not n100000_results:
        print(f"WARNING: No solutions found for N=100,000")
        print(f"Found solutions for: {set(r['size'] for r in results)}")
        print("\nShowing all available solutions:")
        n100000_results = results
    
    # Get N from results
    n = n100000_results[0]['size']
    optimal_length, grid_n, grid_m = calculate_optimal_length_square_grid(n)
    
    print(f"Instance: N = {n:,} cities")
    print(f"Grid Configuration: ~{grid_n}x{grid_m} (sqrt({n}) ~= {grid_n})")
    print(f"Optimal Length (theoretical): {optimal_length:,.2f} grid units")
    print(f"(Assumes unit grid spacing = 1.0 between adjacent cities)")
    print()
    print("-" * 80)
    print("INDIVIDUAL SOLUTIONS:")
    print("-" * 80)
    
    # Analyze each solution
    distances = []
    solution_times = []
    gaps = []
    
    for i, result in enumerate(n100000_results):
        gap = calculate_gap(result['distance'], optimal_length)
        gaps.append(gap)
        distances.append(result['distance'])
        solution_times.append(result['solution_time'])
        
        print(f"\nSolution {i} ({result['file']}):")
        print(f"  Distance: {result['distance']:,.2f} grid units")
        print(f"  Gap: {gap:.4f}%")
        print(f"  Solution Time: {result['solution_time']:.2f} seconds")
        print(f"  Route Length: {result['route_length']:,} cities")
        print(f"  Unique Cities: {result['unique_cities']:,}")
        print(f"  Route Complete: {result['route_complete']}")
    
    # Calculate statistics
    print()
    print("=" * 80)
    print("STATISTICAL SUMMARY:")
    print("=" * 80)
    
    if len(distances) > 0:
        print(f"\nDistance Statistics:")
        print(f"  Best (Minimum): {min(distances):,.2f} grid units")
        print(f"  Worst (Maximum): {max(distances):,.2f} grid units")
        print(f"  Average (Mean): {statistics.mean(distances):,.2f} grid units")
        if len(distances) > 1:
            print(f"  Median: {statistics.median(distances):,.2f} grid units")
            print(f"  Standard Deviation: {statistics.stdev(distances):,.2f} grid units")
    
    if len(gaps) > 0:
        print(f"\nGap Percentage Statistics:")
        print(f"  Best Gap: {min(gaps):.4f}%")
        print(f"  Worst Gap: {max(gaps):.4f}%")
        print(f"  Average Gap: {statistics.mean(gaps):.4f}%")
        if len(gaps) > 1:
            print(f"  Median Gap: {statistics.median(gaps):.4f}%")
            print(f"  Standard Deviation: {statistics.stdev(gaps):.4f}%")
    
    if len(solution_times) > 0:
        print(f"\nSolution Time Statistics:")
        print(f"  Fastest: {min(solution_times):.2f} seconds")
        print(f"  Slowest: {max(solution_times):.2f} seconds")
        print(f"  Average: {statistics.mean(solution_times):.2f} seconds")
        if len(solution_times) > 1:
            print(f"  Median: {statistics.median(solution_times):.2f} seconds")
            print(f"  Total Time: {sum(solution_times):.2f} seconds")
    
    # Quality assessment
    print()
    print("=" * 80)
    print("QUALITY ASSESSMENT:")
    print("=" * 80)
    
    if len(gaps) > 0:
        avg_gap = statistics.mean(gaps)
        best_gap = min(gaps)
        worst_gap = max(gaps)
        
        print(f"\nGap Analysis:")
        print(f"  Average Gap: {avg_gap:.4f}%")
        print(f"  Best Gap: {best_gap:.4f}%")
        print(f"  Worst Gap: {worst_gap:.4f}%")
        
        # Publication standards
        print(f"\nPublication Quality Assessment:")
        if avg_gap <= 0.5:
            quality = "EXCELLENT"
            assessment = "Near-optimal, competitive with exact solvers. Suitable for top-tier journals."
        elif avg_gap <= 1.0:
            quality = "VERY GOOD"
            assessment = "Strong results for large-scale TSP. Suitable for strong papers."
        elif avg_gap <= 2.0:
            quality = "ACCEPTABLE"
            assessment = "Publishable quality. Comparable to state-of-the-art methods."
        elif avg_gap <= 3.0:
            quality = "MARGINAL"
            assessment = "May need justification. Other contributions should be strong."
        else:
            quality = "NEEDS IMPROVEMENT"
            assessment = "Gap > 3% is generally not acceptable for publication."
        
        print(f"  Quality Rating: {quality}")
        print(f"  Assessment: {assessment}")
        
        # Compare with literature
        print(f"\nComparison with Literature (for 100,000 cities):")
        print(f"  Your Average Gap: {avg_gap:.4f}%")
        print(f"  LKH-3: 0.1-1.0%")
        print(f"  HyperNS: 0.5-2.0%")
        print(f"  Grid TSP Approximation: 1.0-2.0%")
        print(f"  UNiCS: 1.0-2.0%")
        
        if avg_gap <= 0.5:
            print(f"  [OK] Your results are EXCELLENT - better than most state-of-the-art methods")
        elif avg_gap <= 1.0:
            print(f"  [OK] Your results are VERY GOOD - competitive with LKH-3")
        elif avg_gap <= 2.0:
            print(f"  [OK] Your results are ACCEPTABLE - comparable to HyperNS and Grid TSP methods")
        else:
            print(f"  [WARNING] Your results need improvement to match state-of-the-art")
    
    # Performance assessment
    print()
    print("=" * 80)
    print("PERFORMANCE ASSESSMENT:")
    print("=" * 80)
    
    if len(solution_times) > 0:
        avg_time = statistics.mean(solution_times)
        print(f"\nSolution Time Analysis:")
        print(f"  Average Time: {avg_time:.2f} seconds")
        print(f"  Fastest Solution: {min(solution_times):.2f} seconds")
        
        if avg_time <= 10.0:
            perf_rating = "EXCELLENT"
            perf_assessment = "Very fast for 100K cities - suitable for real-time applications"
        elif avg_time <= 30.0:
            perf_rating = "VERY GOOD"
            perf_assessment = "Fast - suitable for interactive applications"
        elif avg_time <= 60.0:
            perf_rating = "GOOD"
            perf_assessment = "Acceptable - suitable for batch processing"
        else:
            perf_rating = "NEEDS OPTIMIZATION"
            perf_assessment = "May be too slow for large-scale applications"
        
        print(f"  Performance Rating: {perf_rating}")
        print(f"  Assessment: {perf_assessment}")
    
    print()
    print("=" * 80)
    print("END OF REPORT")
    print("=" * 80)

if __name__ == "__main__":
    main()

