# BAVCL.Tests

Automated tests and performance benchmarks for [BAVCL](../BAVCL).

## Layout

```
BAVCL.Tests/
  BAVCL.Tests.sln
  BAVCL.Tests.csproj          xUnit tests (references BAVCL)
  xunit.runner.json
  Tests/                      unit and integration tests
  BAVCL.Benchmarks/           BenchmarkDotNet project (separate exe)
```

The benchmark project is nested here but excluded from the test assembly (`Compile Remove` in `BAVCL.Tests.csproj`).

## Tests

```bash
dotnet test                                    # default (excludes IO)
dotnet test -p:IncludeIOTests=true --filter Category=IO
```

## Benchmarks

See [BAVCL.Benchmarks/README.md](BAVCL.Benchmarks/README.md).

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SortBenchmarks*
```

## Requirements

- .NET 10 SDK
- CUDA-capable GPU for GPU benchmarks
- Python 3 + matplotlib (optional, for `BAVCL.Benchmarks/Scripts/plot_benchmarks.py`)
