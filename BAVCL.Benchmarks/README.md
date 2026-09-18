# BAVCL.Benchmarks

Performance benchmarks for [BAVCL](../BAVCL). Job config (warmup/iterations) lives in `Program.cs`.

## Layout

```
BAVCL.Benchmarks/
  Program.cs                 entry point
  Infrastructure/            CSV exporter, categories, migration
  Suites/<category>/         benchmark classes grouped by category
  Benchmarks/<category>/     CSV output (gitignored)
  Probes/Probe/              probe CSV output (gitignored, auto-pruned)
  Reports/                   gate/decision markdown (gitignored)
BAVCL.Benchmarks.Viewer/     Blazor Server chart viewer
```

## Run benchmarks

From the `BAVCL.Tests` repo root:

```bash
# Mask compaction A/B gate (→ Reports/*-mask-compaction-ab-gate-report.md)
dotnet run -c Release --project BAVCL.Benchmarks -- --mask-compaction-ab-report

# xUnit accuracy parity (Category=AB)
dotnet test BAVCL.Tests.csproj --filter Category=AB

# CPU legacy vs CPU scan (→ Benchmarks/Mask/<timestamp>.csv)
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *MaskCompactionAbCpuBenchmarks*

# GPU legacy gather vs GPU scan
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *MaskCompactionAbGpuBenchmarks*

# All sorting benchmarks (104 cases: 96 sort/argsort + 8 shape)
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SortBenchmarks* *SortShapeBenchmarks*
```

Do not pass `--job short` unless you intentionally want a second job — it doubles the case count (e.g. 104 → 208).

Each run writes one CSV to `Benchmarks/<Category>/{timestamp}.csv` (suite name stays in the CSV). Probe suites write to `Probes/Probe/` and are auto-pruned after 14 days.

## View results

```bash
dotnet run --project BAVCL.Benchmarks.Viewer
```

The viewer loads CSVs automatically from `BAVCL.Benchmarks/Benchmarks/`. Use **Refresh** after running new benchmarks.

## One-time migration

If you have legacy markdown reports under `Results/`:

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --migrate-results
```

To reorganize existing CSVs into `Benchmarks/<category>/{timestamp}.csv`:

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --relocate-runs
```

## Categories

| Category | Suites |
|----------|--------|
| `Sort` | Sort*, *Argsort*, SortShape* |
| `Vector` | Vector*, SpanRead*, AllX*, Dot* |
| `Stats` | Sum*, Var*, RowReduce*, StatsRead*, DiffViaCumSum* |
| `Memory` | Cache*, MemoryTransfer* |
| `Mask` | MaskCompaction* |
| `Graph` | GraphComparison* |
| `Probe` | *Probe* (ephemeral output) |

Results are machine-dependent — use for relative comparison across runs on the same hardware.
