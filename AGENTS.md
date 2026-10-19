# Agent instructions (BAVCL.Tests)

## Test catalog (required)

When you **add, remove, or materially change** any test under [`Tests/`](Tests/) (excluding [`Tests/Helpers/`](Tests/Helpers/)), update [`docs/TEST_CATALOG.md`](docs/TEST_CATALOG.md) in the **same change**.

The catalog is a **readable** behavioral index: a developer should understand what is being verified **without opening the test file**. One **Given / Then** block per xUnit test method (`[Fact]` / `[Theory]`).

**Authoring rule:** Write each entry by **reading the test method** (arrange, act, assert). Use full sentences and normal word spacing. Do **not** run generators or paste PascalCase method tails as prose (e.g. never “negativerangewithpositivestep adjustsstep”). Do **not** add `scripts/` that auto-fill Given/Then.

### Do not catalog

- Files under `Tests/Helpers/` (fixtures, factories, reference data).

### Document structure

Single file [`docs/TEST_CATALOG.md`](docs/TEST_CATALOG.md):

1. Short intro (IO filter, GPU notes, **no LaTeX** — use plain language and code spans).
2. **Table of contents** with links to each `##` area (mirror `Tests/` folders).
3. For each area:
   - `## {area}` — optional **Coverage gaps (manual)**.
   - `### {TestClassName}` — one section per `*Tests.cs` class.
   - `#### `{MethodName}` — `{TestClassName}`` — one block per test method.

### Entry template

```markdown
#### `SelectVarianceReducePath_PicksExpectedPath` — `KernelSharedMemoryTests`

- **Source:** `Tests/Core/Kernels/KernelSharedMemoryTests.cs` · `[Theory]`
- **Given** A per-block shared memory byte budget `limit` for variance kernel path selection.
- **Then** `SelectVarianceReducePath(limit)` returns the expected `VarianceReducePath` (`SharedScratch` vs `WarpTree`).
- **Cases:**
  - limit=10_000, expectedPathId=0 → `SharedScratch`
  - limit=3071, expectedPathId=1 → `WarpTree`
```

**Given** — concrete setup in plain English (data shapes, device state, invalid inputs). Use normal spacing and sentences. Never paste PascalCase method tails as one word (bad: “negativerangewithpositivestep”; good: “stop is below start with a positive step”).

**Then** — what is asserted (expected values, exceptions, CPU parity, tolerances like `1e-4` or “|actual − expected| < 1e-4”). **Do not use LaTeX** (`\(...\)`, `\[...\]`); it does not render in most Markdown viewers here. Use words, ASCII, or backticks.

**When** — omit unless the action is unclear from the name.

**`[Theory]`** — each **Cases** line must explain parameters (name=value), not only raw `InlineData` tuples.

**Source line** — file path, `[Fact]` or `[Theory]`, optional `GpuSerial`, optional `Category=IO` or `Category=AB`.

### Where to edit

- Maintain **[`docs/TEST_CATALOG.md`](docs/TEST_CATALOG.md)** only (single source of truth).
- Do not add catalog generator scripts under `scripts/`.

### Checklist before finishing

- [ ] Given/Then are understandable without reading the test file.
- [ ] Every new/changed/removed test method has a matching catalog block.
- [ ] TOC still lists all `##` sections.
- [ ] Traits and collections match the test class.
- [ ] Footer **Last audited** method count matches `####` headings.
- [ ] No LaTeX delimiters in the catalog.

### Examples

**New `[Fact]`**

```diff
+#### `RentFloat_ReturnsEntity` — `BufferEntityTests`
+
+- **Source:** `Tests/Core/Memory/BufferEntityTests.cs` · `[Fact]`
+- **Given** The float buffer pool for the current GPU.
+- **Then** `Rent(8)` yields an entity with `View.Length == 8` and `BufferId > 0`.
```

**New `[Theory]`**

```diff
+#### `Sum_MatchesSumX` — `GlobalReduceTests`
+
+- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Theory]` · `GpuSerial`
+- **Given** A random `Vector` whose length is `length`.
+- **Then** `Sum()` and `SumX()` agree within `1e-4`.
+- **Cases:**
+  - length=1
+  - length=4096
```

## Tests (commands)

```bash
dotnet test                                    # default (excludes Category=IO)
dotnet test -p:IncludeIOTests=true --filter Category=IO
```

Many tests require a CUDA GPU (`GpuSerial` collection).
