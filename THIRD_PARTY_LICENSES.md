# Third-Party Licences

Third-party components used by BAVCL.Tests and BAVCL.Benchmarks. Separate from the BAVCL licence in [LICENSE.md](LICENSE.md).

## NuGet (direct references)

| Package | Version | Licence |
|---------|---------|---------|
| Microsoft.NET.Test.Sdk | 17.12.0 | [MIT](https://opensource.org/license/mit) |
| xunit | 2.9.3 | [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) |
| xunit.runner.visualstudio | 2.8.2 | [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) |
| FluentAssertions | 6.12.0 | [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0) |
| coverlet.collector | 6.0.4 | [MIT](https://opensource.org/license/mit) |
| BenchmarkDotNet | 0.14.0 | [MIT](https://opensource.org/license/mit) |

Transitive packages inherit licences from their upstream packages.

## Optional scripts

`BAVCL.Benchmarks/Scripts/plot_benchmarks.py` may use matplotlib (BSD-style) and numpy (BSD-3-Clause).
