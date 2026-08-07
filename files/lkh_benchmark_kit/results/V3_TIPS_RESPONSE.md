# V3 response to Tips ChatGPT + Tips DeepSeek

## Stance
It is difficult to satisfy every reviewer demand (especially proprietary source + full ablation overnight).
V3 does the **polite best** that is scientifically honest: complete the reviewable specification,
remove self-sabotaging “incomplete algorithm” language, tone down defensiveness, fix presentation
bugs, and keep clear about what is not yet claimed.

## ChatGPT tips (score 61) → V3 action
| Tip | Action |
|-----|--------|
| Missing equations / parameters | Added §2.8.1 explicit update rules + Autobench defaults |
| Need ablation | Honest limitation + Table 1 as K-probe; **no fake ablation table** |
| DLL reproducibility | Clarified binary + scripts + 2.8.1; no false “full source” claim |
| 25M risky | One calm preliminary-feasibility paragraph |
| Table 1 vs Table 2 | Clear non-defensive “different questions” note |
| Statistics | Kept mean±SD; pointed to CSVs; no invented CIs |
| Presentation | Deduped DOIs (removed duplicate hyperlink fields); softened AI declaration; page numbers |

## DeepSeek tips (score 78) → V3 action
| Tip | Action |
|-----|--------|
| Remove “algorithm incomplete” admission | Removed; replaced with fallback + ablation-depth limitations |
| Tone Table 1 / 25M | Done |
| Move fallback caveat | Moved out of §2.8 lead complexity claim into Limitations |
| Clarify gaps | Rewrote §3.1 naming (Gap_LB / Gap_opt / Gap_BKS / Gap_rel) |
| Explain 22 GB memory | Added process WorkingSet clarification near §3.7 |
| Strengthen 5× budget | Strengthened without hostility to LKH |
| Page numbers | Added |
| Release source | Cannot unilaterally open proprietary DLL; stated future-desirable + 2.8.1 |

## Still not claimed (on purpose)
- Full open-source release
- Full factorial ablation results
- Same-instance LKH at 500k/1M beyond layout-equivalent grids
- ACM TAPS template re-flow (still content draft; venue = TALG experimental track, JEA closed)
