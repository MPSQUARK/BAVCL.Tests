# BAVCL.Samples

Domain-realistic workloads comparing **eager** BAVCL (per-op GPU launch + sync) with the **captured graph** path (`Graph.Capture` → `Compile` → `Feed` / `Run`) on the same graph-enabled build.

## BAVCL reference

Projects in this repo reference the graph worktree:

`..\BAVCL-graph\BAVCL.csproj` from `BAVCL.Tests.csproj` (`graph-experimental` branch; `..\..\BAVCL-graph` from `BAVCL.Samples` / `BAVCL.Benchmarks`).

Point those references at `..\BAVCL\BAVCL.csproj` instead when benchmarking merged main.

## Samples

| Script | Domain | Workload |
|--------|--------|----------|
| `Scripts/InterpolationLerp.csx` | Math / datascience | `y = y0·(1−t) + y1·t` |
| `Scripts/StatsLogTransform.csx` | Statistics | `y = Log(Abs(x) + ε)` |
| `Scripts/GraphicsReinhard.csx` | Graphics | `c = c / (1 + c)` Reinhard tone map |
| `Scripts/NeuralDenseLayer.csx` | Neural / LA | `y = ReLU(W·x + b)` |
| `Scripts/SolverJacobiStep.csx` | Linear solver | `x ← x + ω·(b − A·x)` |

Shared logic lives in `Workloads/*.cs` (`ISampleWorkload`: eager run, graph capture, feed, parity check).

### Parity

Each workload checks `max |eager − graph| < 1e-4` before timing. xUnit coverage: `Tests/Graphs/GraphSampleParityTests.cs`.

### Sync count (architecture)

- **Eager:** one `Synchronize()` per GPU op in the chain (several per iteration).
- **Graph run:** one sync at end of `Run()` plus feed upload syncs.

## Run dotnet-script demos

Build Release first (scripts `#r` the DLLs):

```bash
dotnet build -c Release BAVCL-graph/BAVCL.csproj
dotnet build -c Release BAVCL.Samples/BAVCL.Samples.csproj

dotnet script BAVCL.Samples/Scripts/InterpolationLerp.csx
```

Repeat for the other scripts under `Scripts/`.

## Benchmarks

From the `BAVCL.Tests` repo root (GPU required):

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *GraphComparison*
```

Report: `BAVCL.Benchmarks/Benchmarks/Graph/<timestamp>.csv`

Methods per sample:

- `{Sample}_Eager` — full eager pipeline each iteration
- `{Sample}_Graph_Run` — `Feed` + `Run` (plan compiled in `GlobalSetup`)
- `{Sample}_Graph_Compile` — capture + compile each iteration

`N` = 100 / 10_000 / 1_000_000 (`BenchmarkSizes`) for 1D samples. MatMul samples use the same param column; at `N = 1_000_000` the matrix order is capped to `K = 1024` (a dense 1M×1M matrix cannot be allocated).

## View results

```bash
dotnet run --project BAVCL.Benchmarks.Viewer
```

Load `BAVCL.Benchmarks/Benchmarks` and filter by category `Graph`.
