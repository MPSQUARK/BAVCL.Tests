# Results

Historical BenchmarkDotNet reports, one markdown file per run.

## Layout

```
Results/
  <SuiteName>/
    YYYY-MM-DD_HHMMSS-<tag>.md
  archive/                   original flat exports (pre-migration)
  incoming/                  drop zone for new flat exports to migrate

Plots/                       generated PNGs (gitignored), sibling of Results/
  <SuiteName>/
    MethodName.png
    cpu_gpu_*.png
```

## Naming

| Pattern | Example |
|---------|---------|
| Standard run | `2026-08-02_161137-baseline.md` |
| Tagged baseline | `2026-08-02_141531-post-refactor-gpu-baseline.md` |
| July 2026 suite baselines | `2026-07-19_202020-july-2026-baseline.md` |

## SortBenchmarks archive

| File | Description |
|------|-------------|
| `2026-08-02_140000-first-gpu-baseline-pre-refactor.md` | First GPU sort run (70 benchmarks, 5 N steps, pre-refactor) |
| `2026-08-02_141531-post-refactor-gpu-baseline.md` | Post-refactor GPU baseline (float radix, argsort 2D reuse) |
| `2026-08-02_161137-baseline.md` | Trimmed suite — sort only, N = 100 / 10k / 1M (48 benchmarks) |

## Commands

```bash
# Run benchmarks
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SortBenchmarks*

# Plot (from BAVCL.Tests repo root)
pip install -r BAVCL.Benchmarks/Scripts/requirements-plot.txt
python BAVCL.Benchmarks/Scripts/plot_benchmarks.py --suite SortBenchmarks --cpu-gpu
python BAVCL.Benchmarks/Scripts/plot_benchmarks.py --suite VectorFactoryBenchmarks

# Migrate any new flat exports dropped in Results/incoming/
python BAVCL.Benchmarks/Scripts/migrate_legacy_reports.py
```
