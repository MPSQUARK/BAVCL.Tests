# BAVCL.Benchmarks

Performance benchmarks for CPU/GPU coherence, span read APIs, and memory-manager hot paths.

## Run

```bash
dotnet run -c Release --project BAVCL.Benchmarks
```

Filter a class:

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SpanReadBenchmarks*
```

## Benchmark classes

| Class | Purpose |
|-------|---------|
| `SpanReadBenchmarks` | `GetCpuReadOnlySpan` vs `GetReadOnlySpan` vs `ToArray()` on GPU-resident vectors (1K / 100K / 1M) |
| `StatsReadBenchmarks` | `Sum`, `Var`, `Min` after span migration |
| `CacheUpdateBenchmarks` | `UpdateCache()` same-length upload |
| `MemoryTransferBenchmarks` | `SyncCPU`, GPU-resident pull, `UpdateCache`, allocate+upload |

Results are machine-dependent. Use `[MemoryDiagnoser]` output to confirm reduced heap allocations on span paths.
