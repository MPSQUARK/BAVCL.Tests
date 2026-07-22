# BAVCL.Benchmarks

Performance benchmarks for CPU/GPU coherence, span read APIs, and memory-manager hot paths.

## Run

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --filter "*"
```

Filter a class:

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SpanReadBenchmarks*
```

Reports are written as GitHub-flavoured markdown under `BaselineResults/results/` (markdown only; no HTML/CSV/log files).

## Benchmark classes

| Class | Purpose |
|-------|---------|
| `SpanReadBenchmarks` | `GetCpuReadOnlySpan` vs `RetrieveReadOnlySpan` vs `ToArray()` on GPU-resident vectors (1K / 100K / 1M) |
| `StatsReadBenchmarks` | `Sum`, `Var`, `Min` after span migration |
| `CacheUpdateBenchmarks` | `UpdateCache()` same-length upload |
| `MemoryTransferBenchmarks` | `SyncCPU` cold vs no-op at 1K/10K/100K; `UpdateCache`, allocate+upload |
| `VectorCreationBenchmarks` | `new Vector` cached/non-cached, `Zeros` |
| `VectorFactoryBenchmarks` | `Arange`, `Linspace`, `Fill`, `Ones`, `Append` / `Append_IP`, `Merge` at 1K/10K/100K |
| `VectorStructuralBenchmarks` | `Transpose`, `Dot`, `Concat` |

Results are machine-dependent. Use `[MemoryDiagnoser]` output to confirm reduced heap allocations on span paths.
