# ACM_TSP_Paper_V2 — agreed improvements from ChatGPT & DeepSeek verdicts

## Verdict synthesis

| Source | Overall | Take seriously? |
|--------|---------|-----------------|
| ChatGPT | Reject / major reconstruction | **Yes** on contradictions (10M/100s vs Table 2), missing operational detail, Table1≠Table2 timing, overclaimed squared-distance novelty, niche vs LKH memory |
| DeepSeek | Strong / JEA-ready after ~7 fixes | **Yes** on pending language, defensive abstract, 5× justification, memory explanation, N=1000 note, positive framing |

**Author (this build) judgment:** DeepSeek understates structural risks ChatGPT flags; ChatGPT overstates “desk reject” if contradictions and positioning are fixed. V2 implements all *manuscript-text* fixes both agree on, plus ChatGPT’s contradiction/niche repairs. It does **not** invent Algorithm‑1 equations (symbols may be OMML-only / incomplete in source)—that remains an open author task called out in Limitations.

## Implemented in V2

1. Removed intro claim “10M≈100s / 25M≈300s” as if same Autobench protocol as Table 2; replaced with Autobench≤1M primary + exploratory §3.6.
2. Softened contribution bullet on “extreme-scale 25M near-linear” and squared-distance “discrimination”.
3. Abstract: removed defensive “not part of primary evidence” / “do not claim dominance”; positive complementary framing.
4. Table 2 note: removed “pending mega sweep”; clarified Gap_LB vs Tier protocol; N=1000 outlier; peak-memory measurement meaning.
5. §3.7: removed “pending re-measurement”.
6. §3.6: labeled 25M/≈300s as exploratory non-Autobench demonstration (different timing scope).
7. Table 1 caption/note: timing is K-sensitivity probe, not full Table 2 Autobench sweep (explains ≪1664s).
8. Squared-distance prose: ordering preserved; amplification is numerical/tie-break, not a new nearer-neighbour rule.
9. Neighbourhood wording: fixed-K sparse neighbourhood with dynamically modulated edge costs.
10. §4.5: justify 5×DVD budget; explain DVD peak WS vs LKH PeakWorkingSet; niche = protocol/quality/diversity not memory win vs LKH.
11. Table 6: all gaps on **EUC LB ≈ 2N** (unified denominator).
12. Complexity: fallback caveat (nearest-outside-NK may be superlinear if naïvely scanned).
13. Limitations: Algorithm specification / open equations; LKH memory honesty.
14. Cover letter draft for JEA (`ACM_JEA_CoverLetter_V2.txt`).

## Still open (not silently invented)

- Restore/complete Algorithm 1 equations & default numeric parameters from author source.
- Ablation (with/without modulation, squared vs Euclidean).
- Same-instance LKH at 500k/1M (currently layout-equivalent).
- Stronger baselines (greedy-edge / 2-opt) beyond NN.
- Full open-source release (DLL remains).
