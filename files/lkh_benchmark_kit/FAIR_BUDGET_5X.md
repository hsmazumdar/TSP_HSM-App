# LKH fair-budget protocol (updated 2026-08-06 — SSD port ready)

## Rule
**LKH wall-clock budget = 5 × DVD-TSP `ElapsedSec` on the same instance.**

Enforced in `03_run_lkh_benchmark.ps1`:
1. LKH `TIME_LIMIT` + `TOTAL_TIME_LIMIT` = budget
2. **Hard process kill** at budget + 15 s
3. **Progress every 30 s** → console + `OutCsv` with `.progress.log` suffix
4. **Crash recovery** → re-run same command; skips CSV-complete runs; recovers orphan `.tour`

Also: `CANDIDATE_SET_TYPE=NEAREST-NEIGHBOR`, `SUBGRADIENT=NO`, `MAX_TRIALS=50`.

## Port note
See **`PORT_SSD_OTHER_PC.txt`** in this folder (copy-paste resume commands).

## Example (N=100000)
DVD ≈ 183.5 s → budget ≈ **918 s/run**.

```powershell
$kit = "<SSD>:\PaperSubmissions2026\_COAP_TSP_31072026\Next_Journal_Attempt\files\lkh_benchmark_kit"
$dvdJson = "<SSD>:\PaperSubmissions2026\TSP_Paper_17072026\experiments\results\lkh_compare_n100000\single_n100000_p2_k1000_20260806_091909\result.json"
cd $kit
.\03_run_lkh_benchmark.ps1 -LkhExe "$kit\LKH.exe" -TspDir "$kit\tsp_instances_n100000" `
  -DvdResultJson $dvdJson -TimeMultiplier 5 -ProgressSec 30 -Runs 3 `
  -OutCsv "$kit\results\lkh_results_n100000_5x.csv"
```
