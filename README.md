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

## Code coverage

[Coverlet](https://github.com/coverlet-coverage/coverlet) via `coverlet.collector` and `CodeCoverage.runsettings` (library assemblies only, Cobertura output). IDE **Analyze Code Coverage** can use the same runsettings file.

```bash
dotnet test -c Release --settings CodeCoverage.runsettings --collect:"XPlat Code Coverage" --results-directory TestResults/coverage
```

Cobertura XML is written under `TestResults/coverage/**/coverage.cobertura.xml` (gitignored).

## Benchmarks

See [BAVCL.Benchmarks/README.md](BAVCL.Benchmarks/README.md).

```bash
dotnet run -c Release --project BAVCL.Benchmarks -- --filter *SortBenchmarks*
```

## Requirements

- .NET 10 SDK
- CUDA-capable GPU for GPU benchmarks
- Python 3 + matplotlib (optional, for `BAVCL.Benchmarks/Scripts/plot_benchmarks.py`)

Third-party licences: [THIRD_PARTY_LICENSES.md](THIRD_PARTY_LICENSES.md).
