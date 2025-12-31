# Experimental Design for Research Paper
## Required N, K, and Number of Trials

## Executive Summary

This document specifies the experimental design parameters (N, K, number of trials) required for a comprehensive research paper on the TSP solver. The design ensures statistical significance, scalability demonstration, and parameter sensitivity analysis.

---

## 1. Problem Sizes (N) - Number of Cities

### 1.1 Core Experimental Instances

**Required for Main Results Section:**

| Grid Size | Cities (N) | Purpose | Priority | Runs Needed |
|-----------|-----------|---------|----------|-------------|
| 10×10 | 100 | Baseline validation | **High** | 10 instances × 10 runs = 100 |
| 20×20 | 400 | Small-scale validation | **High** | 10 instances × 10 runs = 100 |
| 50×50 | 2,500 | Medium-scale validation | **High** | 10 instances × 10 runs = 100 |
| 100×100 | 10,000 | Large-scale performance | **High** | 10 instances × 10 runs = 100 |
| 200×200 | 40,000 | Very large-scale | **High** | 10 instances × 10 runs = 100 |
| 316×316 | 100,000 | Million-city preparation | **High** | 10 instances × 10 runs = 100 |
| 500×500 | 250,000 | Extreme scale | **High** | 10 instances × 10 runs = 100 |
| 707×707 | 500,000 | Million-city preparation | **Medium** | 5 instances × 10 runs = 50 |
| 1000×1000 | 1,000,000 | Maximum scale | **High** | 5 instances × 10 runs = 50 |

**Total Core Runs: 800 runs**

### 1.2 Scalability Analysis Instances

**For Scaling Plots (Time vs. N, Memory vs. N):**

| Cities (N) | Purpose | Runs Needed |
|-----------|---------|-------------|
| 100 | Baseline | 5 runs |
| 1,000 | Small | 5 runs |
| 10,000 | Medium | 5 runs |
| 100,000 | Large | 5 runs |
| 500,000 | Very Large | 5 runs |
| 1,000,000 | Maximum | 5 runs |

**Total Scalability Runs: 30 runs**

### 1.3 Parameter Sensitivity Instances

**For K Parameter Analysis:**

| Cities (N) | Purpose | Runs Needed |
|-----------|---------|-------------|
| 10,000 | Medium-scale K testing | 4 K values × 5 runs = 20 |
| 250,000 | Large-scale K testing | 4 K values × 5 runs = 20 |

**Total Parameter Sensitivity Runs: 40 runs**

---

## 2. K Values (k-Nearest Neighbors)

### 2.1 Default K Value

**Primary K Value:**
- **K = 1000** (used for all main experiments)

**Rationale:**
- Provides best balance of quality (0-0.5% gap) and performance
- Sufficient for optimal neighbor selection in square grids
- Memory-efficient: O(N×K) = O(N) for fixed K

### 2.2 Parameter Sensitivity K Values

**For Parameter Sensitivity Analysis (Experiment 6):**

| K Value | Purpose | Instances to Test |
|---------|---------|------------------|
| 100 | Minimum viable | N=10K, N=250K |
| 500 | Medium | N=10K, N=250K |
| 1000 | **Default (optimal)** | N=10K, N=250K |
| 2000 | Maximum | N=10K, N=250K |

**Total K Sensitivity Runs: 4 K values × 2 N values × 5 runs = 40 runs**

---

## 3. Number of Trials (Runs per Configuration)

### 3.1 Statistical Significance Requirements

**For Publication-Quality Results:**

| Experiment Type | Instances per Size | Runs per Instance | Total Runs per Size | Statistical Purpose |
|----------------|-------------------|------------------|-------------------|---------------------|
| **Main Results** | 10 instances | 10 runs | 100 runs | Mean, std dev, confidence intervals |
| **Scalability** | 1 instance | 5 runs | 5 runs | Time/memory scaling |
| **Parameter Sensitivity** | 1 instance | 5 runs | 5 runs | Parameter impact |
| **Ablation Study** | 1 instance | 5 runs | 5 runs | Component contribution |

### 3.2 Rationale for 10 Runs per Instance

**Statistical Justification:**
- **Mean Estimation**: 10 runs provide reliable mean estimate
- **Standard Deviation**: Sufficient for variance estimation
- **Confidence Intervals**: 95% CI with 10 samples: t-distribution (df=9)
- **Outlier Detection**: Multiple runs identify anomalies
- **Reproducibility**: Demonstrates consistency

**Minimum Acceptable:**
- **5 runs**: Acceptable for scalability plots
- **10 runs**: Recommended for main results
- **20+ runs**: Ideal for statistical tests (t-tests, ANOVA)

### 3.3 Rationale for 10 Instances per Size

**Instance Diversity:**
- **Random Presentation Order**: Different city ordering
- **Grid Variations**: Slight variations in grid generation
- **Robustness**: Tests algorithm invariance
- **Generalization**: Demonstrates consistent performance

**Minimum Acceptable:**
- **5 instances**: Acceptable for large instances (N ≥ 100K)
- **10 instances**: Recommended for all sizes
- **20+ instances**: Ideal for comprehensive validation

---

## 4. Complete Experimental Matrix

### 4.1 Main Results Matrix

| N (Cities) | Grid Size | Instances | Runs/Instance | Total Runs | Priority |
|-----------|-----------|-----------|---------------|------------|----------|
| 100 | 10×10 | 10 | 10 | 100 | **High** |
| 400 | 20×20 | 10 | 10 | 100 | **High** |
| 2,500 | 50×50 | 10 | 10 | 100 | **High** |
| 10,000 | 100×100 | 10 | 10 | 100 | **High** |
| 40,000 | 200×200 | 10 | 10 | 100 | **High** |
| 100,000 | 316×316 | 10 | 10 | 100 | **High** |
| 250,000 | 500×500 | 10 | 10 | 100 | **High** |
| 500,000 | 707×707 | 5 | 10 | 50 | Medium |
| 1,000,000 | 1000×1000 | 5 | 10 | 50 | **High** |

**Total Main Results: 800 runs**

### 4.2 Parameter Sensitivity Matrix (K Values)

| N (Cities) | K Values | Runs per K | Total Runs |
|-----------|----------|------------|------------|
| 10,000 | 100, 500, 1000, 2000 | 5 | 20 |
| 250,000 | 100, 500, 1000, 2000 | 5 | 20 |

**Total K Sensitivity: 40 runs**

### 4.3 Scalability Analysis Matrix

| N (Cities) | Purpose | Runs |
|-----------|---------|------|
| 100 | Baseline | 5 |
| 1,000 | Small | 5 |
| 10,000 | Medium | 5 |
| 100,000 | Large | 5 |
| 500,000 | Very Large | 5 |
| 1,000,000 | Maximum | 5 |

**Total Scalability: 30 runs**

### 4.4 Ablation Study Matrix

| Component Variant | N (Cities) | Runs |
|------------------|-----------|------|
| d² vs. d (linear) | 10K, 100K | 5 each = 10 |
| With/without punishment | 10K, 100K | 5 each = 10 |
| Template vs. brute force | 10K, 100K | 5 each = 10 |

**Total Ablation: 30 runs**

---

## 5. Total Experimental Requirements

### 5.1 Complete Run Count

| Experiment Type | Total Runs |
|----------------|------------|
| Main Results (9 sizes) | 800 |
| Parameter Sensitivity (K) | 40 |
| Scalability Analysis | 30 |
| Ablation Study | 30 |
| **TOTAL** | **900 runs** |

### 5.2 Time Estimation

**Per Run Time (approximate):**
- N=100: < 0.1 sec
- N=10K: ~0.3 sec
- N=100K: ~35 sec (from recent run)
- N=250K: ~90 sec
- N=500K: ~180 sec
- N=1M: ~360 sec (6 min)

**Total Estimated Time:**
- Main Results: ~50-60 hours
- Parameter Sensitivity: ~2-3 hours
- Scalability: ~1-2 hours
- Ablation: ~1-2 hours
- **TOTAL: ~55-70 hours** (can be parallelized)

### 5.3 Storage Requirements

**Per Run Output:**
- Route solution file (.tsp): ~N × 4 bytes (city indices)
- For N=1M: ~4 MB per run

**Total Storage:**
- 900 runs × average 1 MB = ~900 MB
- Plus cache files: ~2-3 GB
- **TOTAL: ~3-4 GB**

---

## 6. Minimum Acceptable Experimental Design

### 6.1 For Initial Submission (Minimum Viable)

**If time-constrained, minimum acceptable design:**

| N (Cities) | Instances | Runs/Instance | Total Runs |
|-----------|-----------|---------------|------------|
| 100 | 5 | 5 | 25 |
| 10,000 | 5 | 5 | 25 |
| 100,000 | 5 | 5 | 25 |
| 1,000,000 | 3 | 5 | 15 |
| **TOTAL** | | | **90 runs** |

**K Values:**
- Default K=1000 only (no sensitivity analysis)

**Time Estimate: ~10-15 hours**

### 6.2 For Strong Paper (Recommended)

**Use complete design from Section 4:**
- All 9 N values
- 10 instances × 10 runs each
- K sensitivity analysis
- Ablation study
- **Total: 900 runs**

---

## 7. Experimental Execution Plan

### 7.1 Phase 1: Core Results (Week 1-2)

**Priority: High**
- Run main results for all 9 N values
- 10 instances × 10 runs = 100 runs per size
- **Total: 800 runs**

### 7.2 Phase 2: Parameter Analysis (Week 3)

**Priority: Medium**
- K sensitivity: 4 K values × 2 N values × 5 runs
- **Total: 40 runs**

### 7.3 Phase 3: Scalability & Ablation (Week 4)

**Priority: Medium**
- Scalability analysis: 6 N values × 5 runs
- Ablation study: 3 variants × 2 N values × 5 runs
- **Total: 60 runs**

### 7.4 Phase 4: Analysis & Reporting (Week 5)

**Priority: High**
- Statistical analysis
- Generate figures and tables
- Write results section

---

## 8. Quality Assurance

### 8.1 Validation Checks

**After Each Run:**
- [ ] Solution length recorded
- [ ] Execution time recorded
- [ ] Route completeness verified (N+1 cities)
- [ ] Gap percentage calculated
- [ ] Solution file saved

**After Each Instance Size:**
- [ ] Mean gap ≤ 0.5% (target)
- [ ] Standard deviation < 0.1%
- [ ] All runs completed successfully
- [ ] Results consistent across instances

### 8.2 Statistical Requirements

**For Publication:**
- **Mean Gap**: Report with 95% confidence interval
- **Standard Deviation**: Report for all metrics
- **Statistical Tests**: t-test for gap ≤ 0.5% hypothesis
- **Outlier Analysis**: Identify and report anomalies

---

## 9. Deliverables

### 9.1 Required Tables

1. **Main Results Table**: Gap statistics for all N values
2. **Performance Table**: Time breakdown for all N values
3. **Parameter Sensitivity Table**: K impact on quality/time
4. **Comparison Table**: vs. state-of-the-art methods

### 9.2 Required Figures

1. **Gap vs. N**: Line plot with error bars
2. **Time vs. N**: Log-log plot (scaling)
3. **Memory vs. N**: Line plot
4. **K Sensitivity**: Line plots (quality vs. K, time vs. K)
5. **Comparison Chart**: Bar chart vs. baselines

### 9.3 Data Files

1. **Raw Data**: CSV with all 900 runs
2. **Aggregated Data**: Summary statistics
3. **Solution Files**: Sample .tsp files for each size

---

## 10. Recommendations

### 10.1 Essential Experiments (Must Have)

1. **N = 100, 10K, 100K, 1M** (4 sizes minimum)
2. **K = 1000** (default)
3. **10 runs per instance** (statistical significance)
4. **5-10 instances per size** (robustness)

### 10.2 Recommended Experiments (Should Have)

1. **All 9 N values** (comprehensive coverage)
2. **K sensitivity** (parameter justification)
3. **Ablation study** (component contribution)
4. **Scalability analysis** (theoretical validation)

### 10.3 Optional Experiments (Nice to Have)

1. **Rectangular grids** (extension)
2. **Non-uniform spacing** (robustness)
3. **Multiple random seeds** (reproducibility)
4. **Comparison with exact solver** (small N only)

---

## Summary

**For a Strong Research Paper:**

- **N Values**: 9 sizes (100 to 1M cities)
- **K Values**: 1000 (default) + 4 values for sensitivity (100, 500, 1000, 2000)
- **Total Runs**: ~900 runs
- **Time Required**: ~55-70 hours (can be parallelized)
- **Storage**: ~3-4 GB

**For Minimum Viable Paper:**

- **N Values**: 4 sizes (100, 10K, 100K, 1M)
- **K Values**: 1000 only
- **Total Runs**: ~90 runs
- **Time Required**: ~10-15 hours

---

**Document Version**: 1.0  
**Date**: December 2025  
**Status**: Complete Experimental Design Specification

