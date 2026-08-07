# BAVCL.Benchmarks



Performance benchmarks for [BAVCL](../BAVCL). Job config (warmup/iterations) lives in `Program.cs`.



## Layout



```

BAVCL.Benchmarks/

  Program.cs                 entry point

  Infrastructure/            shared config, exporters, globals

  Suites/                    one *Benchmarks.cs class per file

  Scripts/                   plotting and migration helpers

  Results/                   dated markdown reports

  Plots/                     generated charts (gitignored)

```



## Run



From the `BAVCL.Tests` repo root:



```bash

# All sorting benchmarks (104 cases: 96 sort/argsort + 8 shape)
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SortBenchmarks* *SortShapeBenchmarks*

# Sort API only (96 cases)
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SortBenchmarks*

```

Do not pass `--job short` unless you intentionally want a second job — it doubles the case count (e.g. 104 → 208).

Each run writes one report to `Results/<Suite>/<timestamp>-baseline.md`.

### Sort benchmarks (96 + 8 = 104 cases)



Allocating `SortAsc` / `SortDesc` and `ArgsortAsc` / `ArgsortDesc` (CPU) and `SortAscX` / `SortDescX` and `ArgsortAscX` / `ArgsortDescX` (GPU), int and float, 1D and 2D, at N = 100 / 10_000 / 1_000_000.



## Plot results



```bash

pip install -r BAVCL.Benchmarks/Scripts/requirements-plot.txt

python BAVCL.Benchmarks/Scripts/plot_benchmarks.py --suite SortBenchmarks --cpu-gpu

python BAVCL.Benchmarks/Scripts/plot_benchmarks.py --suite VectorFactoryBenchmarks

python BAVCL.Benchmarks/Scripts/plot_benchmarks.py --method IntSort_Gpu_Asc_1D --suite SortBenchmarks

# Layout sensitivity (GPU int sort, in-place, no per-iter clone)
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SortShapeBenchmarks*

```



Charts land in `Plots/<Suite>/`:

- **Per-method** — historical runs (linear Y, log X; unit labels ns/μs/ms/s; dashed guides at 1 ns, 1 μs, 1 ms, 1 s)

- **CPU vs GPU** (`cpu_gpu_*.png`) — 1D and 2D subplots for the latest run (SortBenchmarks only)



## Benchmark classes



| Class | Methods × N | Notes |

|-------|-------------|-------|

| `SortBenchmarks` | 32 × 3 = 96 | Sort/argsort asc/desc, CPU/GPU, int/float, 1D/2D |
| `SortShapeBenchmarks` | 2 × 2 × 2 = 8 | GPU int sort in-place; matrix shape sweep |

| `SpanReadBenchmarks` | 3 × 3 = 9 | Span read paths |

| `StatsReadBenchmarks` | 3 × 3 = 9 | Sum / Var / Min |

| `CacheUpdateBenchmarks` | 1 × 3 = 3 | UpdateCache |

| `MemoryTransferBenchmarks` | 4 × 3 = 12 | Sync / upload |

| `VectorCreationBenchmarks` | 3 × 3 = 9 | Create / Zeros |

| `VectorFactoryBenchmarks` | 6 × 3 = 18 | Arange, Linspace, etc. |

| `VectorArithmeticBenchmarks` | 4 × 3 = 12 | Add / Sub / Mul / Div |

| `VectorUnaryBenchmarks` | 4 × 3 = 12 | AbsX, ReverseX, etc. |

| `VectorStructuralBenchmarks` | 3 × 3 = 9 | Transpose, Dot, Concat |

| `Vector3GeometryBenchmarks` | 3 × 3 = 9 | Cross, Magnitude, Distance |



All classes use `[Params(100, 10_000, 1_000_000)]`. Warmup 1–2, iterations 2–3.



Results are machine-dependent — use for relative comparison across runs on the same hardware.

