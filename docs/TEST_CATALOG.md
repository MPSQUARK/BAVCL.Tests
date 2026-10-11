# BAVCL test catalog (Given / Then)

Behavioral inventory of xUnit tests under [`Tests/`](../Tests/). Entries are **hand-authored** from test source (see [AGENTS.md](../AGENTS.md)) — not generated from method names. Default `dotnet test` excludes `Category=IO`. Many tests require a CUDA GPU (`GpuSerial`).

## How to read

- **Given** — what is set up (data, shapes, device state).
- **Then** — what the test asserts (including tolerances in plain language).
- Numeric checks use **plain text** (e.g. tolerance `1e-4`, |actual − expected|), not LaTeX — so the file renders in any Markdown viewer.

## Table of contents

- [Core / Enums](#core--enums)
- [Core / GPU](#core--gpu)
- [Core / Generators](#core--generators)
- [Core / Kernels](#core--kernels)
- [Core / Mask](#core--mask)
- [Core / Memory](#core--memory)
- [Core / Scan](#core--scan)
- [Core / Sorting](#core--sorting)
- [Core / Statistics](#core--statistics)
- [Core / Vector](#core--vector)
- [Core / VectorBase](#core--vectorbase)
- [Core / VectorInt](#core--vectorint)
- [Experimental](#experimental)
- [Extensions](#extensions)
- [Geometric](#geometric)
- [Geometric / Vector3](#geometric--vector3)
- [Graphs](#graphs)
- [IO](#io)
- [Modules / Statistics](#modules--statistics)
- [Services](#services)
- [Utility](#utility)

## Core / Enums

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `OperationsEnumTests`

#### `Operations_ContainsExpectedValues` — `OperationsEnumTests`

- **Source:** `Tests/Core/Enums/OperationsEnumTests.cs` · `[Fact]`
- **Given** The `Operations` enum used by geometric ops.
- **Then** `Enum.GetNames<Operations>()` includes `add`, `subtract`, `multiply`, `divide`, `pow`, `magnitude`, and `distance`.

#### `Axis_ContainsRowAndColumn` — `OperationsEnumTests`

- **Source:** `Tests/Core/Enums/OperationsEnumTests.cs` · `[Fact]`
- **Given** The `Axis` enum for row/column reductions.
- **Then** `Enum.GetNames<Axis>()` is exactly `Row` and `Column` (order not significant).

#### `IoFormatters_ExposeSingletonDefault` — `OperationsEnumTests`

- **Source:** `Tests/Core/Enums/OperationsEnumTests.cs` · `[Fact]`
- **Given** Built-in IO formatter types for `Vector`, `Vector3`, and `Mask`.
- **Then** `JsonFormatter` / `XmlFormatter` implement `IFormatter<Vector>`; `CsvFormatter` implements `IFormatter<Vector3>` and `IFormatter<Mask>`; each `Default` singleton is stable and exposes extension `.json`, `.xml`, `.csv`, or `.txt` as documented.

## Core / GPU

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `GPUManagerTests`

#### `Default_ReturnsInitializedGpu` — `GPUManagerTests`

- **Source:** `Tests/Core/GPU/GPUManagerTests.cs` · `[Fact]`
- **Given** The shared test fixture has constructed a `GPU` via `GPUManager`.
- **Then** `Gpu` is non-null and `GPUManager.IsInitialized` is true.

#### `GetGPU_WithInvalidMemoryCap_Throws` — `GPUManagerTests`

- **Source:** `Tests/Core/GPU/GPUManagerTests.cs` · `[Fact]`
- **Given** Calls to `GPUManager.GetGPU` with memory cap fraction `0` and `1` (outside the allowed range).
- **Then** Both throw `Exception` with a message containing `Memory Cap`.

#### `GetGPU_ForceCpu_ReturnsCpuAccelerator` — `GPUManagerTests`

- **Source:** `Tests/Core/GPU/GPUManagerTests.cs` · `[Fact]`
- **Given** `GPUManager.GetGPU(forceCPU: true)`.
- **Then** The returned wrapper’s ILGPU accelerator type is `AcceleratorType.CPU`.

### `GPUMemoryTests`

#### `GetMemUsage_ReturnsParseableValue` — `GPUMemoryTests`

- **Source:** `Tests/Core/GPU/GPUMemoryTests.cs` · `[Fact]`
- **Given** A cached GPU `Vector` `[1, 2, 3]` exists on the device.
- **Then** `Gpu.GetMemUsage()` returns a string that parses as a `long` >= 0.

#### `CachedVector_HasNonZeroId` — `GPUMemoryTests`

- **Source:** `Tests/Core/GPU/GPUMemoryTests.cs` · `[Fact]`
- **Given** A GPU-backed `Vector` filled with `[1 … 5]`.
- **Then** `vector.ID` is greater than zero (cache entry was assigned).

### `KernelExceptionTests`

#### `TestSQRTKernel_ThrowsKernelNotCompiledException` — `KernelExceptionTests`

- **Source:** `Tests/Core/GPU/KernelExceptionTests.cs` · `[Fact]`
- **Given** A call to the test-only `Gpu.TestSqrtIP_Kern` entry point (kernel not registered in production builds).
- **Then** Throws `KernelNotCompiledException` mentioning `TestSqrtIP_Kern`.

#### `TestMYSQRTKernel_ThrowsKernelNotCompiledException` — `KernelExceptionTests`

- **Source:** `Tests/Core/GPU/KernelExceptionTests.cs` · `[Fact]`
- **Given** A call to `Gpu.TestMySqrtIP_Kern` (another unregistered test kernel).
- **Then** Throws `KernelNotCompiledException` mentioning `TestMySqrtIP_Kern`.

### `StatisticsDomainLoadTests`

#### `SumX_WithoutStatisticsDomain_ThrowsKernelNotCompiledException` — `StatisticsDomainLoadTests`

- **Source:** `Tests/Core/GPU/StatisticsDomainLoadTests.cs` · `[Fact]` · `GpuSerial`
- **Given** statistics GPU kernels **not** loaded on the device.
- **Then** throws `KernelNotCompiledException` with message matching `*KernelDomain.Statistics*`

#### `SumX_SmallN_WithoutStatisticsDomain_ThrowsGroupedCompensatedKernel` — `StatisticsDomainLoadTests`

- **Source:** `Tests/Core/GPU/StatisticsDomainLoadTests.cs` · `[Fact]` · `GpuSerial`
- **Given** statistics GPU kernels **not** loaded on the device.
- **Then** throws `KernelNotCompiledException` with message matching `*KernelDomain.Statistics*`

#### `DotX_WithoutStatisticsDomain_ThrowsKernelNotCompiledException` — `StatisticsDomainLoadTests`

- **Source:** `Tests/Core/GPU/StatisticsDomainLoadTests.cs` · `[Fact]` · `GpuSerial`
- **Given** statistics GPU kernels **not** loaded on the device.
- **Then** throws `KernelNotCompiledException` with message matching `*KernelDomain.Statistics*`

## Core / Generators

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `GeneratorsTests`

#### `Arange_Int_ReturnsExpectedSequence` — `GeneratorsTests`

- **Source:** `Tests/Core/Generators/GeneratorsTests.cs` · `[Fact]`
- **Given** Integer `ArangeArray` from `0` to `4` with step `1` (stop exclusive).
- **Then** The materialized array is `[0, 1, 2, 3]`.

#### `Arange_Double_NegativeRangeWithPositiveStep_AdjustsStep` — `GeneratorsTests`

- **Source:** `Tests/Core/Generators/GeneratorsTests.cs` · `[Fact]`
- **Given** Double `ArangeArray` from `0` toward `-4` with a **positive** step `1` (stop is below start).
- **Then** The generator negates the step and yields `[0, -1, -2, -3]` (same rule as NumPy-style arange).

#### `Linspace_Int_IncludesBothEndpoints` — `GeneratorsTests`

- **Source:** `Tests/Core/Generators/GeneratorsTests.cs` · `[Fact]`
- **Given** `LinspaceArray(0, 8, 5)` — five evenly spaced integers from 0 to 8 inclusive.
- **Then** Length is 5, first element is `0`, last is `8`, and the full sequence is `[0, 2, 4, 6, 8]`.

#### `Linspace_Double_ReturnsEvenlySpacedValues` — `GeneratorsTests`

- **Source:** `Tests/Core/Generators/GeneratorsTests.cs` · `[Fact]`
- **Given** `LinspaceArray(0d, 4d, 3)` — three points from 0.0 to 4.0 inclusive.
- **Then** Length is 3, endpoints are `0` and `4`, and the middle value is `2`.

#### `Arange_LazyEnumerable_MatchesMaterializedArray` — `GeneratorsTests`

- **Source:** `Tests/Core/Generators/GeneratorsTests.cs` · `[Fact]`
- **Given** Lazy `GeneratorsModule.Arange(2, 8, 2)` enumerated to an array.
- **Then** Result matches the direct array API: `[2, 4, 6]`.

#### `Linspace_SinglePoint_ReturnsStartOnly` — `GeneratorsTests`

- **Source:** `Tests/Core/Generators/GeneratorsTests.cs` · `[Fact]`
- **Given** `LinspaceArray(7, 99, 1)` — only one sample requested.
- **Then** Output is the single-element array `[7]` (start value; end is not duplicated when count is 1).

## Core / Kernels

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `KernelSharedMemoryTests`

#### `VarianceReduceSharedBytes_IsExpectedLayout` — `KernelSharedMemoryTests`

- **Source:** `Tests/Core/Kernels/KernelSharedMemoryTests.cs` · `[Fact]`
- **Given** The static layout helper for shared-scratch variance reduction.
- **Then** `VarianceReduceSharedBytes()` equals `256 * (sizeof(int) + 2*sizeof(float))` bytes.

#### `VarianceReduceWarpTreeBytes_IsExpectedLayout` — `KernelSharedMemoryTests`

- **Source:** `Tests/Core/Kernels/KernelSharedMemoryTests.cs` · `[Fact]`
- **Given** The static layout helper for warp-tree variance reduction.
- **Then** `VarianceReduceWarpTreeBytes()` matches `VarianceWarpLeaderSlots * (sizeof(int) + 2*sizeof(float))`.

#### `SelectVarianceReducePath_PicksExpectedPath` — `KernelSharedMemoryTests`

- **Source:** `Tests/Core/Kernels/KernelSharedMemoryTests.cs` · `[Theory]`
- **Given** A candidate per-block **shared memory byte budget** (`limit`) for variance reduction (kernel path selection in `KernelSharedMemory`).
- **Then** `KernelSharedMemory.SelectVarianceReducePath(limit)` returns the expected `VarianceReducePath` (0=`SharedScratch`, 1=`WarpTree`).
- **Cases:**
  - limit=10_000, expectedPathId=0 → `SharedScratch`
  - limit=3072, expectedPathId=0 → `SharedScratch`
  - limit=3071, expectedPathId=1 → `WarpTree`
  - limit=96, expectedPathId=1 → `WarpTree`

#### `SelectVarianceReducePath_WhenBelowWarpTree_Throws` — `KernelSharedMemoryTests`

- **Source:** `Tests/Core/Kernels/KernelSharedMemoryTests.cs` · `[Theory]`
- **Given** A shared memory `limit` **below 96 bytes** (minimum required for the warp-tree variance path).
- **Then** throws `NotSupportedException` with message matching `*at least 96 bytes*`
- **Cases:**
  - limit=95
  - limit=0

## Core / Mask

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `MaskAggregatesTests`

#### `CountTrue_MatchesPopcount` — `MaskAggregatesTests`

- **Source:** `Tests/Core/Mask/MaskAggregatesTests.cs` · `[Fact]` · `GpuSerial`
- **Given** A GPU `Mask` with bits `[true, false, true, true, false, false, true]`.
- **Then** `CountTrue()` returns `4`.

#### `CountTrueX_MatchesCpu` — `MaskAggregatesTests`

- **Source:** `Tests/Core/Mask/MaskAggregatesTests.cs` · `[Fact]` · `GpuSerial`
- **Given** A 70-bit mask where index `i` is true when `i % 3 == 0`.
- **Then** `CountTrueX()` equals `CountTrue()` on the same mask.

#### `Any_All_FullAndSparse` — `MaskAggregatesTests`

- **Source:** `Tests/Core/Mask/MaskAggregatesTests.cs` · `[Fact]` · `GpuSerial`
- **Given** All-true mask length 3 and sparse mask `[false, false, true, false]`.
- **Then** Full mask: `Any()` and `All()` are true; sparse mask: `Any()` is true and `All()` is false.

#### `AnyX_AllX_MatchCpu` — `MaskAggregatesTests`

- **Source:** `Tests/Core/Mask/MaskAggregatesTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Mask `[true, false, false, true]`.
- **Then** `AnyX()` equals `Any()` and `AllX()` equals `All()`.

#### `FirstLastTrueIndex_RespectTailWord` — `MaskAggregatesTests`

- **Source:** `Tests/Core/Mask/MaskAggregatesTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Length-33 mask with a single true bit at index `32` (spans a 32-bit word boundary).
- **Then** `FirstTrueIndex`, `LastTrueIndex`, `FirstTrueIndexX`, and `LastTrueIndexX` all return `32`.

#### `FirstTrueIndex_AllFalse_ReturnsMinusOne` — `MaskAggregatesTests`

- **Source:** `Tests/Core/Mask/MaskAggregatesTests.cs` · `[Fact]` · `GpuSerial`
- **Given** All-false mask of length 3.
- **Then** `FirstTrueIndex()` and `FirstTrueIndexX()` return `-1`.

### `MaskCompactionAbTests`

#### `FilterX_WorksWithMaskDefaultWorkloadOnly` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** `KernelModuleLoader` loads `KernelWorkloads.Default` on a CPU GPU before mask compaction.
- **Then** Result is close to `[10f, 30f]` (test tolerance).

#### `FilterX_MatchesReference_1D` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `Vector` `[10f, 20f, 30f, 40f]` and `Mask` `[true, false, true, false]`.
- **Then** `MaskCompaction.FilterX` synced values match reference `FilterX` within test tolerance.

#### `PartitionX_MatchesReference_1D` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `Vector` `[1f, 2f, 3f, 4f]` and `Mask` `[true, false, true, false]`.
- **Then** `PartitionX` true/false lane vectors each match reference `PartitionX` within test tolerance.

#### `Where_MatchesReference_CpuPath` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `Mask` `[false, true, false, true, true]`.
- **Then** `mask.Where()` index array equals reference `WhereCpu` on the same mask.

#### `WhereX_MatchesReference_Sparse` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `Mask` `[false, true, false, true, true]`.
- **Then** `WhereX` index array equals reference `Where` on the same mask.

#### `FilterX_MatchesReference_TailWord` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** Length-33 float vector with values `0..32` and mask true where index is even (spans a 32-bit word boundary).
- **Then** `FilterX` synced values match reference `FilterX` within test tolerance.

#### `VectorInt_FilterX_MatchesReference` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `VectorInt` `[1, 2, 3, 4]` and `Mask` `[true, false, true, false]`.
- **Then** `FilterX` synced values equal reference `FilterX`.

#### `PartitionCpu_MatchesLegacyCpu` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `Vector` `[1f, 2f, 3f, 4f]` and `Mask` `[true, false, true, false]`; CPU paths with `syncToGpu: false`.
- **Then** `MaskCompactionCpu.Partition` true/false lanes match legacy reference `Partition` within test tolerance.

#### `FilterCpu_MatchesLegacyCpu` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `Vector` `[10f, 20f, 30f, 40f]` and `Mask` `[true, false, true, false]`; CPU paths with `syncToGpu: false`.
- **Then** `MaskCompactionCpu.Filter` synced values match legacy reference `Filter` within test tolerance.

#### `Filter_Operator_MatchesReference` — `MaskCompactionAbTests`

- **Source:** `Tests/Core/Mask/MaskCompactionAbTests.cs` · `[Fact]` · `GpuSerial` · Category=AB
- **Given** GPU `Vector` `[5f, 6f, 7f, 8f]` and `Mask` `[true, true, false, false]`.
- **Then** Filter via `vector | mask` matches reference `FilterX` within test tolerance.

### `MaskOperationsTests`

#### `MaskBitwise_AndOrXor` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true, false]` and `[true, true, false, false]`.
- **Then** Element-wise `&`, `|`, and `^` on masks match `[true,false,false,false]`, `[true,true,true,false]`, `[false,true,true,false]` respectively.

#### `MaskUnary_NotSetClear` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true]`.
- **Then** `~mask` inverts bits; unary `+mask` sets all true; unary `-mask` clears all.

#### `VectorCompare_OperatorsAndMethods` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Numeric vector data `[1f, 2f, 3f, 4f]` / `[0f, 2f, 5f, 4f]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorScalarCompare` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Numeric vector data `[0.5f, 1.5f, 2.5f]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorCompare_NanEquals` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Numeric vector data `[float.NaN, 1f]` / `[float.NaN, 2f]`. inputs include **NaN** floats (IEEE compare semantics).
- **Then** `CompareEqualsX` treats NaN as equal to NaN; ordering comparisons with `>` are false for NaN pairs.

#### `VectorMask_AndOperator` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true, false]` and `[1f, 2f, 3f, 4f]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorFilter_OperatorAndIndexer` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true, false]` and `[10f, 20f, 30f, 40f]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorPartition_SplitsLanes` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true, false]` and `[1f, 2f, 3f, 4f]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorPartition_ExtensionMethod_MatchesOperator` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** GPU `Vector` `[1f, 2f, 3f, 4f]` and `Mask` `[true, false, true, false]`.
- **Then** `PartitionX` true/false lanes match `/` operator partition within test tolerance.

#### `VectorPartition_AllTrue_ReturnsFullVectorAndEmptyFalseLanes` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** GPU `Vector` `[1f, 2f, 3f]` and all-true mask length 3.
- **Then** True lanes match `[1f, 2f, 3f]` within tolerance; false lanes have `Length == 0` and `Columns == 0`.

#### `VectorPartition_AllFalse_ReturnsEmptyTrueLanesAndFullFalseVector` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** GPU `Vector` `[1f, 2f, 3f]` and all-false mask length 3.
- **Then** True lanes have `Length == 0` and `Columns == 0`; false lanes match `[1f, 2f, 3f]` within tolerance.

#### `VectorPartition_2D_BroadcastMask_SplitsInRowMajorOrder` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** 2×2 GPU `Vector` `[1f, 2f, 3f, 4f]` and broadcast `Mask` `[true, false]` (`columns: 0`).
- **Then** True lanes are `[1f, 3f]` and false lanes `[2f, 4f]` within test tolerance.

#### `VectorFilter_EmptyMask_ReturnsZeroLength` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** GPU `Vector` `[1f, 2f, 3f]` and all-false mask length 3.
- **Then** Filter result `vector | mask` has `Length == 0` and `Columns == 0`.

#### `MaskNand_MatchesComposed` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** GPU masks `a = [true, true, false]` and `b = [true, false, true]`.
- **Then** Synced `a.Nand(b)` bits equal synced `~(a & b)`.

#### `MaskBitwise_Broadcast_1DTo2D` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false]` and `[true, false, true, false]`.
- **Then** Result equals `[true, false, true, false]`.

#### `VectorMask_Broadcast_1DMaskTo2DVector` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false]` and `[1f, 2f, 3f, 4f]`.
- **Then** Result equals `[1f, 0f, 3f, 0f]`.

#### `MaskComplement_SpanningWords_LeavesPaddingLanesClear` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** 70-bit mask with true lanes where `i % 3 == 0`, then bitwise complement `~mask`.
- **Then** Synced bits equal element-wise negation of the source; padding bits in the final storage word are zero (unused lanes stay clear).

#### `MaskInPlace_Broadcast_1DTo2D` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, true, true, false, false, false]` and `[true, false, true]`.
- **Then** Result equals `[true, false, true, false, false, false]`.

#### `MaskInPlace_ResizingLeftOperand_Throws` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true]` and `[true, true, true, false, false, false]`.
- **Then** throws `PerformanceException`

#### `VectorIntCompare_OperatorsAndMethods` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Numeric vector data `[1, 2, 3, 4]` / `[0, 2, 5, 4]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorIntMask_AndOperator` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true, false]` and `[1, 2, 3, 4]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorIntFilter_EmptyMask_ReturnsZeroLength` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** GPU `VectorInt` `[1, 2, 3]` and all-false mask length 3.
- **Then** Filter result `vector | mask` has `Length == 0` and `Columns == 0`.

#### `VectorIntMask_Broadcast_1DMaskTo2DVector` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false]` and `[1, 2, 3, 4]`.
- **Then** Result equals `[1, 0, 3, 0]`.

#### `VectorIntPartition_SplitsLanes` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false, true, false]` and `[1, 2, 3, 4]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

#### `VectorIntPartition_ExtensionMethod_MatchesOperator` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** GPU `VectorInt` `[1, 2, 3, 4]` and `Mask` `[true, false, true, false]`.
- **Then** `PartitionX` true/false lanes equal `/` operator partition synced values.

#### `VectorIntPartition_AllTrue_ReturnsFullVectorAndEmptyFalseLanes` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, true, true]` and `[1, 2, 3]`.
- **Then** Result equals `[1, 2, 3]`.

#### `VectorIntPartition_AllFalse_ReturnsEmptyTrueLanesAndFullFalseVector` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[false, false, false]` and `[1, 2, 3]`.
- **Then** Result equals `[1, 2, 3]`.

#### `VectorIntPartition_2D_BroadcastMask_SplitsInRowMajorOrder` — `MaskOperationsTests`

- **Source:** `Tests/Core/Mask/MaskOperationsTests.cs` · `[Fact]`
- **Given** Boolean masks with fixed patterns `[true, false]` and `[1, 2, 3, 4]`.
- **Then** Each asserted result equals its expected array (see test for `&`, `|`, `^`, etc.).

## Core / Memory

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `BufferEntityTests`

#### `KernelPath_EntityViewUsableInGpuScope` — `BufferEntityTests`

- **Source:** `Tests/Core/Memory/BufferEntityTests.cs` · `[Fact]`
- **Given** `pool.Int.Rent(4)` and a `GpuScope` pin on that entity.
- **Then** Inside the scope, `View.Length` is `4` and `BufferId > 0`.

#### `RentFloat_ReturnsEntity` — `BufferEntityTests`

- **Source:** `Tests/Core/Memory/BufferEntityTests.cs` · `[Fact]`
- **Given** `pool.Float.Rent(8)` on the fixture GPU.
- **Then** `View.Length` is `8` and `BufferId > 0`.

### `BufferPoolTests`

#### `Rent_ReturnsEntityWithNonZeroBufferId` — `BufferPoolTests`

- **Source:** `Tests/Core/Memory/BufferPoolTests.cs` · `[Fact]`
- **Given** `pool.Int.Rent(16)`.
- **Then** `BufferId > 0`, `View.Length == 16`, and `Capacity >= 16`.

#### `Rent_RoundsCapacityToPowerOfTwo` — `BufferPoolTests`

- **Source:** `Tests/Core/Memory/BufferPoolTests.cs` · `[Fact]`
- **Given** `pool.Int.Rent(5)`.
- **Then** `Capacity` is `8` (next power of two).

#### `EntityDispose_ReturnsToPool` — `BufferPoolTests`

- **Source:** `Tests/Core/Memory/BufferPoolTests.cs` · `[Fact]`
- **Given** Rent and dispose an 8-element int entity, then rent again.
- **Then** A second `Rent(8)` succeeds without throwing and yields `BufferId > 0`.

#### `BufferPools_For_ReturnsSameInstancePerGpu` — `BufferPoolTests`

- **Source:** `Tests/Core/Memory/BufferPoolTests.cs` · `[Fact]`
- **Given** Two calls to `BufferPools.For` with the same `GPU`.
- **Then** Both references are the same pool instance.

#### `Pool_ExposesOnlyIntAndFloatLanes` — `BufferPoolTests`

- **Source:** `Tests/Core/Memory/BufferPoolTests.cs` · `[Fact]`
- **Given** The per-GPU `BufferPool` from `BufferPools.For`.
- **Then** `Int` and `Float` lane accessors are non-null.

### `IsStoredTests`

#### `IsStored_ReturnsTrueForCachedVector` — `IsStoredTests`

- **Source:** `Tests/Core/Memory/IsStoredTests.cs` · `[Fact]`
- **Given** GPU `Vector` `[1f, 2f, 3f]` with assigned cache id.
- **Then** `Gpu.IsStored(vector.ID)` is true.

#### `IsStored_ReturnsFalseAfterGcItem` — `IsStoredTests`

- **Source:** `Tests/Core/Memory/IsStoredTests.cs` · `[Fact]`
- **Given** Cached vector then `Gpu.GCItem(id)` on that id.
- **Then** `Gpu.IsStored(id)` is false.

#### `IsStored_ReturnsFalseForUnknownId` — `IsStoredTests`

- **Source:** `Tests/Core/Memory/IsStoredTests.cs` · `[Fact]`
- **Given** Unknown cache id `999_999`.
- **Then** `Gpu.IsStored(999_999u)` is false.

### `VesselPossessionTests`

#### `CreateVessel_StartsAsVessel` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** `Mask.CreateVessel(Gpu, 64)` and `VectorInt.CreateVessel(Gpu)`.
- **Then** Both wrappers report `IsVessel`; mask target id is `0`.

#### `Possess_SoulHasBufferId_Banish_RestoresVessel` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** Int buffer entity possessed into a 64-element mask vessel, then scope disposed (banish).
- **Then** While possessed, soul has `ID > 0` and `ElementCount == 64`; after banish soul id is `0` and vessel is empty again.

#### `Possess_VectorIntSoul_AllowsCpuWrite` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** Entity possessed into `Vessel<VectorInt>`, values written under `CpuScopeAndSync`.
- **Then** `ToArray()` after possession is `[7, 8, 9, 10]`.

#### `Possess_ThenSecondPossessAfterBanish_Succeeds` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** Possess → banish → possess again on the same entity and `VectorInt` vessel.
- **Then** Second possession assigns a live target (`ID > 0`); vessel returns to empty after final banish.

#### `Possess_WhileAlreadyPossessed_Throws` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** Nested `Possess` on the same entity while an outer scope is active.
- **Then** Inner call throws `EntityAlreadyPossessedException`.

#### `Possess_VesselAlreadyInhabited_Throws` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** `Vessel<Mask>` wrapping an inhabited mask (not empty).
- **Then** `Possess` throws `VesselAlreadyInhabitedException`.

#### `Dispose_EntityWhilePossessed_Throws` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** Active possession scope on a rented entity.
- **Then** `entity.Dispose()` throws `EntityDisposeWhilePossessedException`.

#### `Possess_VesselGpuMismatch_Throws` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** Entity on fixture GPU and vessel created on a different `GPU` instance (skipped when only one GPU).
- **Then** `Possess` throws `VesselGpuMismatchException`.

#### `Banish_IsIdempotent` — `VesselPossessionTests`

- **Source:** `Tests/Core/Memory/VesselPossessionTests.cs` · `[Fact]`
- **Given** Possession scope disposed twice after banish.
- **Then** Vessel ends in empty state (`IsVessel` true) without error.

## Core / Scan

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `ScanTests`

#### `CumSumIP_1D_MatchesReference` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU 1D `Vector` initialized to `[3f, 1f, 4f, 1f, 5f]`.
- **Then** After in-place `CumSumIP`, synced values are `[3f, 4f, 8f, 9f, 14f]` within test tolerance.

#### `CumSumExclusiveIP_1D_MatchesReference` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU 1D `Vector` initialized to `[3f, 1f, 4f, 1f, 5f]`.
- **Then** After in-place `CumSumExclusiveIP`, synced values are `[0f, 3f, 4f, 8f, 9f]` within test tolerance.

#### `CumProdIP_1D_MatchesReference` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU 1D `Vector` initialized to `[2f, 3f, 4f]`.
- **Then** After in-place `CumProdIP`, synced values are `[2f, 6f, 24f]` within test tolerance.

#### `CumSumXIP_1D_MatchesCpu` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Identical 1D float data on CPU-path (`CumSumIP`) vs GPU-path (`CumSumXIP`) vectors.
- **Then** Synced GPU values match synced CPU cumulative sum within test tolerance.

#### `CumSumExclusiveXIP_1D_MatchesCpu` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Identical 1D float data; CPU uses `CumSumExclusiveIP`, GPU uses `CumSumExclusiveXIP`.
- **Then** Synced GPU values match synced CPU exclusive prefix sum within test tolerance.

#### `CumSum_2D_PerRow` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 2×2 GPU `Vector` row-major `[1f, 2f, 3f, 4f]` (`columns: 2`).
- **Then** After in-place `CumSumIP`, synced values are `[1f, 3f, 3f, 7f]` within test tolerance (prefix sum per row).

#### `CumSumX_2D_PerRow_MatchesCpu` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 2×2 matrix `[1,2; 10,20]`; CPU `CumSumIP` vs GPU `CumSumXIP`.
- **Then** Synced GPU row-wise prefix sums match CPU within test tolerance.

#### `VectorInt_CumSumXIP_MatchesCpu` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** `VectorInt` `[1,2,3,4,5]`; CPU `CumSumIP` vs GPU `CumSumXIP`.
- **Then** Synced integer arrays are equal.

#### `Diff_AfterCumSumX_RoundTrip` — `ScanTests`

- **Source:** `Tests/Core/Scan/ScanTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 1D data `[3,1,4,1,5]`, apply `CumSumXIP` then `DiffX`.
- **Then** `Diff` length is `data.Length - 1` and values match original tail `data[1..]` within tolerance.

## Core / Sorting

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `ArgsortTests`

#### `ArgsortAsc_1D_DoesNotMutateInput_AndIndicesProduceSortedOrder` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `VectorInt` of 128 random ints (seed `99`), flat layout (`columns: 0`).
- **Then** Input values unchanged after `ArgsortAsc`; returned indices sort each segment in ascending order.

#### `ArgsortDesc_2D_PerRowIndices` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 3×6 GPU `VectorInt` of random ints (seed `99`).
- **Then** `ArgsortDesc` indices sort each row in descending order per original values.

#### `Argsort_Empty_ReturnsEmpty` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Empty GPU `VectorInt` (`cache: false`).
- **Then** `ArgsortAsc` returns indices with `Length == 0`.

#### `Argsort_LengthOne_ReturnsZero` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Numeric vector data `[42]`.
- **Then** Single-element input returns index `[0]`.

#### `ArgsortAscX_MatchesCpuArgsort_Int` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate GPU `VectorInt` copies of 256 random ints (seed `99`), flat layout.
- **Then** `ArgsortAscX` indices produce ascending sorted order; same check passes for CPU `ArgsortAsc`.

#### `ArgsortAscX_DoesNotMutateInput` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `Vector` of 64 random floats (seed `99`).
- **Then** Synced values unchanged after `ArgsortAscX`.

#### `ArgsortDescX_MatchesCpuArgsort_Float2D` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate 2×20 GPU `Vector` copies of random floats (seed `99`).
- **Then** `ArgsortDescX` and CPU `ArgsortDesc` indices each sort every row in descending order.

#### `ArgsortAscIP_Cpu_WritesIntoCallerBuffer_AndDoesNotMutateInput` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `VectorInt` of 128 random ints and a pre-sized indices buffer matching vector shape.
- **Then** Input unchanged; caller indices buffer receives ascending argsort order.

#### `ArgsortAscIP_ShapeMismatch_Throws` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `VectorInt` length 128 and indices buffer one element shorter.
- **Then** `ArgsortAscIP(indices)` throws `ShapeMismatchException`.

#### `ArgsortAscXIP_Gpu_WritesIntoCallerBuffer_AndDoesNotMutateInput` — `ArgsortTests`

- **Source:** `Tests/Core/Sorting/ArgsortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `VectorInt` of 256 random ints and a pre-sized indices buffer matching vector shape.
- **Then** Input unchanged; caller indices buffer receives ascending argsort order from GPU path.

### `SortMemoryPressureTests`

#### `ArgsortAscX_1D_Int_SurvivesRepeatedEvictionPressure` — `SortMemoryPressureTests`

- **Source:** `Tests/Core/Sorting/SortMemoryPressureTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Tight-memory isolated `GPU`; four iterations of length-512 random `VectorInt` (flat, seed `2027`).
- **Then** Each `ArgsortAscX` leaves input unchanged and indices sort values in ascending order.

#### `ArgsortDescX_2D_Int_SurvivesRepeatedEvictionPressure` — `SortMemoryPressureTests`

- **Source:** `Tests/Core/Sorting/SortMemoryPressureTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same tight-memory `GPU`; four iterations of length-512 random `VectorInt` with `columns: 8`.
- **Then** Each `ArgsortDescX` leaves input unchanged and indices sort each row in descending order.

#### `ArgsortAscX_1D_Float_SurvivesRepeatedEvictionPressure` — `SortMemoryPressureTests`

- **Source:** `Tests/Core/Sorting/SortMemoryPressureTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same tight-memory `GPU`; four iterations of length-512 random `Vector` floats (flat).
- **Then** Each `ArgsortAscX` leaves input unchanged and indices sort values in ascending order.

#### `ArgsortDescX_2D_Float_SurvivesRepeatedEvictionPressure` — `SortMemoryPressureTests`

- **Source:** `Tests/Core/Sorting/SortMemoryPressureTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same tight-memory `GPU`; four iterations of length-512 random `Vector` floats with `columns: 8`.
- **Then** Each `ArgsortDescX` leaves input unchanged and indices sort each row in descending order.

### `SortShapeEdgeCaseTests`

#### `SortAscIP_ColumnVector_MatchesArraySort` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `VectorInt` of 512 random ints (`columns: 1`, seed `2026`).
- **Then** After `SortAscIP`, synced values equal `Array.Sort` on the same data.

#### `SortAscXIP_Flat1D_Large_MatchesCpu` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate flat GPU `VectorInt` copies of 100_001 random ints (seed `2026`).
- **Then** After `SortAscXIP` vs CPU `SortAscIP`, synced GPU values equal synced CPU values.

#### `SortAscXIP_ColumnVector_Large_MatchesCpu` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate column-layout (`columns: 1`) GPU `VectorInt` copies of 100_001 random ints.
- **Then** After `SortAscXIP` vs CPU `SortAscIP`, synced GPU values equal synced CPU values.

#### `SortDescXIP_Flat1D_Large_MatchesCpu_Float` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate flat GPU `Vector` copies of 100_001 random floats (seed `2026`).
- **Then** After `SortDescXIP` vs CPU `SortDescIP`, synced GPU values match CPU within test tolerance.

#### `SortDescXIP_ColumnVector_Large_MatchesCpu_Float` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate column-layout GPU `Vector` copies of 100_001 random floats.
- **Then** After `SortDescXIP` vs CPU `SortDescIP`, synced GPU values match CPU within test tolerance.

#### `ArgsortAscX_Flat1D_Large_MatchesCpu` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate flat GPU `VectorInt` copies of 100_001 random ints.
- **Then** `ArgsortAscX` and CPU `ArgsortAsc` indices each yield globally ascending order.

#### `ArgsortAscX_ColumnVector_Large_MatchesCpu` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate column-layout GPU `VectorInt` copies of 100_001 random ints.
- **Then** `ArgsortAscX` and CPU `ArgsortAsc` indices each yield globally ascending order.

#### `ArgsortDescX_Flat1D_Large_MatchesCpu_Float` — `SortShapeEdgeCaseTests`

- **Source:** `Tests/Core/Sorting/SortShapeEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate flat GPU `Vector` copies of 100_001 random floats.
- **Then** `ArgsortDescX` and CPU `ArgsortDesc` indices each yield globally descending order.

### `VectorIntSortTests`

#### `SortAscIP_1D_MatchesArraySort` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU flat `VectorInt` of 256 random ints (seed `42`).
- **Then** After `SortAscIP`, synced values equal `Array.Sort` on the same data.

#### `SortDescIP_1D_MatchesArraySort` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU flat `VectorInt` of 256 random ints (seed `42`).
- **Then** After `SortDescIP`, synced values equal descending `Array.Sort` on the same data.

#### `SortIP_WithOrder_MatchesNamedMethods` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Theory]` · `GpuSerial`
- **Given** random fill using the test's fixed `Random` seed.
- **Then** Agrees with the reference / expected values constructed in the test.
- **Cases:**
  - order=SortOrder.Ascending
  - order=SortOrder.Descending

#### `SortIP_2D_SortsEachRowIndependently` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 4×8 GPU `VectorInt` of random ints (seed `42`).
- **Then** After `SortAscIP`, each row matches per-row ascending `Array.Sort` reference.

#### `SortIP_Empty_IsNoOp` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Empty GPU `VectorInt` (`cache: false`).
- **Then** After `SortAscIP`, vector `Length` remains `0`.

#### `SortIP_LengthOne_IsNoOp` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Numeric vector data `[7]`.
- **Then** Result equals `[7]`.

#### `SortIP_ColumnVector_MatchesFlat1D` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same 128 random ints in flat `VectorInt` vs column layout (`columns: 1`).
- **Then** After `SortAscIP` on both, synced values are equal.

#### `SortAscXIP_MatchesCpuSort_1D` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate flat GPU `VectorInt` copies of 512 random ints.
- **Then** After `SortAscXIP` vs CPU `SortAscIP`, synced GPU values equal synced CPU values.

#### `SortDescXIP_MatchesCpuSort_2D` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate 3×16 GPU `VectorInt` copies of random ints.
- **Then** After `SortDescXIP` vs CPU `SortDescIP`, synced GPU values equal synced CPU values.

#### `SortAsc_AllocatingCpu_DoesNotMutateInput_AndReturnsSortedCopy` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `VectorInt` of 256 random ints.
- **Then** `SortAsc` leaves input unchanged and returns a new vector equal to `Array.Sort` on the data.

#### `SortAscX_AllocatingGpu_DoesNotMutateInput_AndReturnsSortedCopy` — `VectorIntSortTests`

- **Source:** `Tests/Core/Sorting/VectorIntSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `VectorInt` of 256 random ints.
- **Then** `SortAscX` leaves input unchanged and returns a new vector equal to `Array.Sort` on the data.

### `VectorSortTests`

#### `SortAscIP_1D_MatchesArraySort` — `VectorSortTests`

- **Source:** `Tests/Core/Sorting/VectorSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU flat `Vector` of 256 random floats (seed `17`).
- **Then** After `SortAscIP`, synced values match `Array.Sort` within test tolerance.

#### `SortDescIP_2D_SortsEachRowIndependently` — `VectorSortTests`

- **Source:** `Tests/Core/Sorting/VectorSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 3×10 GPU `Vector` of random floats (seed `17`).
- **Then** After `SortDescIP`, each row matches per-row descending sort reference within tolerance.

#### `SortIP_ColumnVector_MatchesFlat1D` — `VectorSortTests`

- **Source:** `Tests/Core/Sorting/VectorSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same 96 random floats in flat `Vector` vs column layout (`columns: 1`).
- **Then** After `SortAscIP` on both, synced values match within test tolerance.

#### `SortAscXIP_MatchesCpuSort_1D` — `VectorSortTests`

- **Source:** `Tests/Core/Sorting/VectorSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate flat GPU `Vector` copies of 512 random floats.
- **Then** After `SortAscXIP` vs CPU `SortAscIP`, synced GPU values match CPU within tolerance.

#### `SortDescXIP_MatchesCpuSort_2D` — `VectorSortTests`

- **Source:** `Tests/Core/Sorting/VectorSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Duplicate 4×12 GPU `Vector` copies of random floats.
- **Then** After `SortDescXIP` vs CPU `SortDescIP`, synced GPU values match CPU within tolerance.

#### `SortAsc_AllocatingCpu_DoesNotMutateInput_AndReturnsSortedCopy` — `VectorSortTests`

- **Source:** `Tests/Core/Sorting/VectorSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `Vector` of 256 random floats.
- **Then** `SortAsc` leaves input unchanged; returned vector matches `Array.Sort` within tolerance.

#### `SortAscX_AllocatingGpu_DoesNotMutateInput_AndReturnsSortedCopy` — `VectorSortTests`

- **Source:** `Tests/Core/Sorting/VectorSortTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU `Vector` of 256 random floats.
- **Then** `SortAscX` leaves input unchanged; returned vector matches `Array.Sort` within tolerance.

## Core / Statistics

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `RollingStatsTests`

#### `RollingSum_K2_MatchesManual` — `RollingStatsTests`

- **Source:** `Tests/Core/Statistics/RollingStatsTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU 1D `Vector` `[1f, 2f, 3f, 4f, 5f]` and rolling window `2`.
- **Then** `RollingSum` synced values are `[1f, 3f, 5f, 7f, 9f]` within test tolerance.

#### `MovingAverage_K3_MatchesPartialEdges` — `RollingStatsTests`

- **Source:** `Tests/Core/Statistics/RollingStatsTests.cs` · `[Fact]` · `GpuSerial`
- **Given** GPU 1D `Vector` `[1f, 2f, 3f, 4f, 5f]` and moving-average window `3`.
- **Then** Synced averages are `[1f, 1.5f, 2f, 3f, 4f]` within test tolerance (partial windows at edges).

#### `RollingSumX_MatchesCpu` — `RollingStatsTests`

- **Source:** `Tests/Core/Statistics/RollingStatsTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same 8-element 1D vector on CPU (`RollingSum`) and GPU (`RollingSumX`) with window `4`.
- **Then** Synced GPU rolling sums match CPU within test tolerance.

#### `RollingSum_2D_PerRow_DoesNotCrossRows` — `RollingStatsTests`

- **Source:** `Tests/Core/Statistics/RollingStatsTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 2×2 GPU `Vector` `[1f, 2f, 100f, 200f]` (`columns: 2`) and rolling window `2`.
- **Then** `RollingSum` synced values are `[1f, 3f, 100f, 300f]` within tolerance (sums stay within each row).

#### `RollingSumX_2D_PerRow_MatchesCpu` — `RollingStatsTests`

- **Source:** `Tests/Core/Statistics/RollingStatsTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 2×2 matrix and window `2`; CPU `RollingSum` vs GPU `RollingSumX`.
- **Then** Synced per-row rolling sums match within test tolerance.

#### `MovingAverageX_MatchesCpu` — `RollingStatsTests`

- **Source:** `Tests/Core/Statistics/RollingStatsTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same 1D vector; CPU `MovingAverage` vs GPU `MovingAverageX` with window `3`.
- **Then** Synced moving averages match within test tolerance.

## Core / Vector

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `ScopeTests`

#### `CpuScope_Begin_nestedScopes_keepActiveCpuUntilOutermostDispose` — `ScopeTests`

- **Source:** `Tests/Core/Vector/ScopeTests.cs` · `[Fact]`
- **Given** A length-3 vector `[1, 2, 3]` with `Residence.Gpu`.
- **Then** While nested `CpuScope()` scopes are open, `Residence` stays `ActiveCpu`; after the outer scope disposes, `Residence` is `Cpu`.

#### `CpuScope_Begin_syncToGpu_uploadsOnDispose` — `ScopeTests`

- **Source:** `Tests/Core/Vector/ScopeTests.cs` · `[Fact]`
- **Given** A GPU-resident vector `[1, 2, 3]` edited through `CpuScopeAndSync()` (index 0 set to `99`).
- **Then** After dispose, `Residence` is `InSync`, CPU `Value` and `Pull()` are close to `[99, 2, 3]`, and the scope exposes an editable view.

#### `CpuScope_Begin_crossContext_throwsWhenGpuScopeActive` — `ScopeTests`

- **Source:** `Tests/Core/Vector/ScopeTests.cs` · `[Fact]`
- **Given** A vector under an open `GpuScope`.
- **Then** Opening `CpuScope()` throws `InvalidOperationException`.

#### `CpuScope_Begin_worksOnCoherenceOnlyCacheable` — `ScopeTests`

- **Source:** `Tests/Core/Vector/ScopeTests.cs` · `[Fact]`
- **Given** A test `ICacheable<float>` with no editable CPU span (coherence-only stub).
- **Then** `CpuScopeAndSync()` sets `ActiveCpu`, calls `SyncCPU`, has no view; on exit `Residence` is `InSync` and `UpdateCache` ran.

#### `CpuScope_View_throwsWhenNoEditableSpan` — `ScopeTests`

- **Source:** `Tests/Core/Vector/ScopeTests.cs` · `[Fact]`
- **Given** Coherence-only `ICacheable<float>` inside `CpuScope()` with `HasView == false`.
- **Then** Reading `scope.View` throws `InvalidOperationException`.

#### `EditCpu_outsideCpuScope_throws` — `ScopeTests`

- **Source:** `Tests/Core/Vector/ScopeTests.cs` · `[Fact]`
- **Given** A normal vector with no open CPU scope.
- **Then** `EditCpu` throws `InvalidOperationException`.

#### `GpuScope_Begin_setsActiveGpuOnModified` — `ScopeTests`

- **Source:** `Tests/Core/Vector/ScopeTests.cs` · `[Fact]`
- **Given** An in-sync vector `[1, 2, 3]`.
- **Then** Inside `GpuScope`, `Residence` is `ActiveGpu` and `LiveCount` is `1`; after dispose, `Residence` is `Gpu` and `LiveCount` is `0`.

### `VectorAllTests`

#### `All_ReturnsTrueWhenNoZeros` — `VectorAllTests`

- **Source:** `Tests/Core/Vector/VectorAllTests.cs` · `[Fact]`
- **Given** Vector `[1, 2, 3]` (no zeros).
- **Then** `All()` is true.

#### `All_ReturnsFalseWhenContainsZero` — `VectorAllTests`

- **Source:** `Tests/Core/Vector/VectorAllTests.cs` · `[Fact]`
- **Given** Vector `[1, 0, 3]`.
- **Then** `All()` is false.

### `VectorArithmeticTests`

#### `UnaryPlus_ReturnsAbs` — `VectorArithmeticTests`

- **Source:** `Tests/Core/Vector/VectorArithmeticTests.cs` · `[Fact]`
- **Given** Vector built from mixed-sign fixture data.
- **Then** Unary `+vector` matches synced `AbsX()` element-wise within test tolerance.

#### `UnaryMinus_NegatesValues` — `VectorArithmeticTests`

- **Source:** `Tests/Core/Vector/VectorArithmeticTests.cs` · `[Fact]`
- **Given** Vector `[1, -2, 3]`.
- **Then** `-vector` syncs to `[-1, 2, -3]`.

#### `VectorVectorOperators_MatchCpuReference` — `VectorArithmeticTests`

- **Source:** `Tests/Core/Vector/VectorArithmeticTests.cs` · `[Theory]`
- **Given** Equal-length float pairs and a binary operator name (`add`, `sub`, `mul`, `div`, `pow`).
- **Then** GPU result synced to CPU matches `CpuReference` for that operator within tolerance.
- **Cases:**
  - a=[1,2,3], b=[4,5,6], op=add
  - a=[10,20,30], b=[1,2,3], op=sub
  - a=[2,3,4], b=[5,6,7], op=mul
  - a=[10,20,30], b=[2,4,5], op=div
  - a=[2,3,4], b=[2,2,2], op=pow

#### `VectorScalarOperators_MatchCpuReference` — `VectorArithmeticTests`

- **Source:** `Tests/Core/Vector/VectorArithmeticTests.cs` · `[Theory]`
- **Given** Vector `[2, 4, 6]` and scalar `scalar`.
- **Then** `vector + scalar`, `vector * scalar`, `scalar - vector`, and `scalar / vector` match element-wise expectations within tolerance.
- **Cases:**
  - scalar=2
  - scalar=-3

#### `Divide_ByZero_ProducesInfinity` — `VectorArithmeticTests`

- **Source:** `Tests/Core/Vector/VectorArithmeticTests.cs` · `[Fact]`
- **Given** Vectors `[1, -2]` and `[0, 0]`.
- **Then** Element-wise division yields `+Infinity` then `-Infinity`.

### `VectorBroadcastErrorTests`

#### `Operator_IncompatibleShapes_Throws` — `VectorBroadcastErrorTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastErrorTests.cs` · `[Theory]`
- **Given** Two GPU float tensors whose shapes cannot broadcast (see **Cases**).
- **Then** `BroadcastReference.CanBroadcast` is false and `a + b` throws `ShapeMismatchException`.
- **Cases:**
  - shapeA=[3,5] vs shapeB=[2,5] (row count mismatch)
  - shapeA=[3,5] vs shapeB=[3,4] (column mismatch)
  - shapeA=[2,3] vs shapeB=[6] (length-6 vector vs 2×3 matrix)
  - shapeA=[4] vs shapeB=[2,3] (length-4 vs 2×3)
  - shapeA=[2,2] vs shapeB=[3,3] (both matrices, incompatible)

#### `OP_IncompatibleShapes_Throws` — `VectorBroadcastErrorTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastErrorTests.cs` · `[Theory]`
- **Given** Same incompatible shape pairs as the operator test.
- **Then** `a.OP(b, Operations.add)` throws `ShapeMismatchException`.
- **Cases:**
  - shapeA=[3,5] vs shapeB=[2,5] (row count mismatch)
  - shapeA=[3,5] vs shapeB=[3,4] (column mismatch)
  - shapeA=[2,3] vs shapeB=[6] (length-6 vector vs 2×3 matrix)
  - shapeA=[4] vs shapeB=[2,3] (length-4 vs 2×3)
  - shapeA=[2,2] vs shapeB=[3,3] (both matrices, incompatible)

#### `IPOP_Incompatible2DShapes_Throws` — `VectorBroadcastErrorTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastErrorTests.cs` · `[Fact]`
- **Given** 2×3 and 3×2 matrices with sequential data.
- **Then** `IPOP(..., add)` throws `ShapeMismatchException`.

#### `IPOP_SmallerLeftOperandWithLargerBroadcastOutput_ThrowsPerformanceException` — `VectorBroadcastErrorTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastErrorTests.cs` · `[Fact]`
- **Given** 1×3 row vector and 2×3 matrix.
- **Then** `row.IPOP(matrix, add)` throws `PerformanceException` mentioning degraded performance.

#### `IPOP_ColumnVectorWithMatrix_ThrowsPerformanceException` — `VectorBroadcastErrorTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastErrorTests.cs` · `[Fact]`
- **Given** 2×1 column vector and 2×3 matrix.
- **Then** `column.IPOP(matrix, add)` throws `PerformanceException`.

### `VectorBroadcastInPlaceTests`

#### `IPOP_Broadcast_MatchesNumPyReference` — `VectorBroadcastInPlaceTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastInPlaceTests.cs` · `[Theory]`
- **Given** Left/right GPU tensors and an in-place `Operations` value; shapes from `IpopBroadcastCases` (m,n ∈ {2,3}).
- **Then** After `IPOP` and `SyncCPU`, length/columns match broadcast output shape and `Value` matches NumPy reference for that op.
- **Cases:**
  - m=2,n=2: 2×2 matrix += 1×2 row (add)
  - m=2,n=3: 2×3 matrix += 1×3 row (add)
  - m=3,n=2: 3×2 matrix += 1×2 row (add)
  - m=3,n=3: 3×3 matrix += 1×3 row (add)
  - m=2,n=2: 2×2 matrix *= length-2 vector when m==n else length-n vector (multiply)
  - m=2,n=3: 2×3 matrix *= length-3 vector (multiply)
  - m=3,n=2: 3×2 matrix *= length-2 vector (multiply)
  - m=3,n=3: 3×3 matrix *= length-3 vector (multiply)

#### `IPOP_ScalarOnMatrix_MatchesNumPy` — `VectorBroadcastInPlaceTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastInPlaceTests.cs` · `[Fact]`
- **Given** 2×3 matrix filled with sequential data starting at 2.
- **Then** `IPOP(3f, multiply)` leaves values matching NumPy scale-by-3.

#### `IPOP_RowBroadcastOnMatrix_MatchesNumPy` — `VectorBroadcastInPlaceTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastInPlaceTests.cs` · `[Fact]`
- **Given** 2×3 matrix and 1×3 row broadcast partner.
- **Then** In-place add matches NumPy broadcast add reference.

### `VectorBroadcastTests`

#### `Operator_Broadcast_MatchesNumPyReference` — `VectorBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastTests.cs` · `[Theory]`
- **Given** Each case uses NumPy-broadcastable shape pairs from `BroadcastTestData.CompatibleBroadcastPairs` (2×3 and 3×4 matrices, row/column/flat-1D layouts with sequential fill data). Operator `op` is one of add/sub/mul/div/pow.
- **Then** Result shape and values match NumPy broadcast reference (`BavclShape.ShouldMatchNumpyShape`).
- **Cases:**
  - op=add across all compatible pairs
  - op=sub across all compatible pairs
  - op=mul across all compatible pairs
  - op=div across all compatible pairs
  - op=pow across all compatible pairs

#### `OP_Static_Broadcast_MatchesNumPyReference` — `VectorBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastTests.cs` · `[Theory]`
- **Given** Each case uses NumPy-broadcastable shape pairs from `BroadcastTestData.CompatibleBroadcastPairs` (2×3 and 3×4 matrices, row/column/flat-1D layouts with sequential fill data).
- **Then** `a.OP(b, add)` matches NumPy broadcast add reference.
- **Cases:**
  - All pairs in `CompatibleBroadcastPairs`

#### `Operator_ReverseOperand_Broadcast_MatchesNumPyReference` — `VectorBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastTests.cs` · `[Theory]`
- **Given** Each case uses NumPy-broadcastable shape pairs from `BroadcastTestData.CompatibleBroadcastPairs` (2×3 and 3×4 matrices, row/column/flat-1D layouts with sequential fill data).
- **Then** `b - a` matches NumPy subtract with broadcast output shape `(shapeB, shapeA)`.
- **Cases:**
  - All pairs in `CompatibleBroadcastPairs`

#### `Broadcast_RowVectorPlusMatrix_MatchesNumPy` — `VectorBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastTests.cs` · `[Fact]`
- **Given** 2×3 matrix and 1×3 row (sequential test data).
- **Then** `matrix + row` and `row + matrix` both match NumPy broadcast add.

#### `Broadcast_1DLengthNPlusMxN_MatchesNumPy` — `VectorBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastTests.cs` · `[Fact]`
- **Given** 2×3 matrix and length-3 vector (1D storage).
- **Then** `matrix + vec` matches NumPy broadcast add.

#### `Broadcast_ColumnPlusMatrix_MatchesNumPy` — `VectorBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastTests.cs` · `[Fact]`
- **Given** 2×3 matrix and 2×1 column.
- **Then** `matrix + col` matches NumPy broadcast add.

#### `EqualShape2D_ElementWise_MatchesNumPy` — `VectorBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorBroadcastTests.cs` · `[Fact]`
- **Given** Two 2×3 matrices of equal shape.
- **Then** Element-wise `a * b` matches NumPy multiply (not matmul).

### `VectorConstructionTests`

#### `Constructor_WithValues_SetsDimensions` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Theory]`
- **Given** Five-element 1D fixture with `cache` true or false.
- **Then** `Length` is 5, `Columns` is 0, `MemorySize` is 5×sizeof(float), synced values match fixture.
- **Cases:**
  - cache=true
  - cache=false

#### `Constructor_WithColumns_StoresMatrixShape` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Fact]`
- **Given** 3×5 matrix fixture (`columns: 5`).
- **Then** `Length` 15, `Columns` 5, `RowCount()` 3, rectangular, not 1D.

#### `Copy_CreatesIndependentVector` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Fact]`
- **Given** Small 1D fixture vector.
- **Then** `Copy()` equals original but is a different instance.

#### `Equals_ReturnsFalseForDifferentValues` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Fact]`
- **Given** Vectors `[1,2,3]` and `[1,2,4]`.
- **Then** `Equals` is false.

#### `Shape_ReturnsLengthAndColumns` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Fact]`
- **Given** 3×5 matrix fixture.
- **Then** `Shape()` is `(3, 5)`.

#### `Flatten_SetsColumnsToZero` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Fact]`
- **Given** 3×5 matrix.
- **Then** After `Flatten()`, `Columns` is 0.

#### `ToVector3_RequiresMultipleOfThree` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Fact]`
- **Given** Nine-element vector (multiple of 3).
- **Then** `ToVector3()` yields length 9 with `Columns` 3.

#### `ToVector3_ThrowsWhenLengthNotMultipleOfThree` — `VectorConstructionTests`

- **Source:** `Tests/Core/Vector/VectorConstructionTests.cs` · `[Fact]`
- **Given** Length-4 vector.
- **Then** `ToVector3()` throws with message containing `multiple of 3`.

### `VectorCrossTests`

#### `Cross_2D_MatMul_MatchesReference` — `VectorCrossTests`

- **Source:** `Tests/Core/Vector/VectorCrossTests.cs` · `[Theory]`
- **Given** Matrices A (m×k) and B (k×n) with sequential fill; `CrossX` is matrix multiply.
- **Then** Result is m×n and matches `BroadcastReference.MatMul`.
- **Cases:**
  - m=2,k=3,n=2
  - m=3,k=2,n=4
  - m=2,k=2,n=2

#### `Cross_MatrixTimes1D_MatchesReference` — `VectorCrossTests`

- **Source:** `Tests/Core/Vector/VectorCrossTests.cs` · `[Theory]`
- **Given** m×k matrix times k×1 column.
- **Then** Output length m, `Columns` 1, values match matmul reference.
- **Cases:**
  - m=2,k=3
  - m=3,k=4

#### `Cross_1DTimesMatrix_MatchesReference` — `VectorCrossTests`

- **Source:** `Tests/Core/Vector/VectorCrossTests.cs` · `[Theory]`
- **Given** 1×k row times k×n matrix.
- **Then** 1×n result matches matmul reference.
- **Cases:**
  - k=2,n=3
  - k=4,n=2

#### `Cross_1DLengthN_TimesMxN_MatchesMatrixVectorProduct` — `VectorCrossTests`

- **Source:** `Tests/Core/Vector/VectorCrossTests.cs` · `[Fact]`
- **Given** 2×3 matrix and 3×1 column.
- **Then** `CrossX` length 2 matches `MatMulVector` reference.

#### `Cross_IncompatibleInnerDimensions_Throws` — `VectorCrossTests`

- **Source:** `Tests/Core/Vector/VectorCrossTests.cs` · `[Fact]`
- **Given** 2×3 and 2×2 matrices.
- **Then** `CrossX` throws `ArgumentException` mentioning inner dimensions.

#### `Cross_EqualShape2D_IsMatMul_NotElementWise` — `VectorCrossTests`

- **Source:** `Tests/Core/Vector/VectorCrossTests.cs` · `[Fact]`
- **Given** 2×2 matrices `[1,2,3,4]` and `[5,6,7,8]`.
- **Then** `CrossX` matches matmul; element-wise `a * b` does not.

### `VectorDotProductTests`

#### `Dot_1D_ReturnsScalarInnerProduct` — `VectorDotProductTests`

- **Source:** `Tests/Core/Vector/VectorDotProductTests.cs` · `[Theory]`
- **Given** 1D float pairs and expected dot product.
- **Then** `Dot` and `DotX` match `expected` (GPU within `1e-4`); matches `BroadcastReference.InnerProduct`.
- **Cases:**
  - a=[1,2,3], b=[4,5,6], expected=32
  - a=[2,0,1], b=[3,4,5], expected=11
  - a=[1,-1,2], b=[3,3,3], expected=6

#### `Dot_Scalar_ReturnsScaledSum` — `VectorDotProductTests`

- **Source:** `Tests/Core/Vector/VectorDotProductTests.cs` · `[Theory]`
- **Given** Vector `[1,2,3]` and scalar.
- **Then** `Dot(scalar)` and `DotX(scalar)` equal sum(data)×scalar within `1e-4` on GPU.
- **Cases:**
  - scalar=2
  - scalar=-3

#### `Dot_EqualLength2D_ReturnsScalarFrobeniusInnerProduct` — `VectorDotProductTests`

- **Source:** `Tests/Core/Vector/VectorDotProductTests.cs` · `[Fact]`
- **Given** Two equal 2×3 matrices.
- **Then** `Dot` is one scalar: sum of element-wise products (Frobenius inner product).

#### `Dot_RowWithColumn_ReturnsScalarInnerProduct` — `VectorDotProductTests`

- **Source:** `Tests/Core/Vector/VectorDotProductTests.cs` · `[Theory]`
- **Given** 2×3 and 3×2 matrices; row `row` of A dotted with column `col` of B.
- **Then** Matches inner product of extracted row/column arrays.
- **Cases:**
  - row=0,col=0
  - row=1,col=1

#### `Dot_UnequalLength_SpecRequiresEqualLength` — `VectorDotProductTests`

- **Source:** `Tests/Core/Vector/VectorDotProductTests.cs` · `[Fact]`
- **Given** Length-3 and length-2 CPU arrays.
- **Then** Reference inner product throws `ArgumentException`; `vectorA.Dot(vectorB)` throws `LengthMismatchException`.

### `VectorFactoriesTests`

#### `Zeros_CreatesZeroFilledVector` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** Five-element zero factory vector.
- **Then** All elements 0; length 5.

#### `Ones_CreatesOneFilledVector` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** Length-4 ones vector.
- **Then** All elements 1.

#### `Fill_CreatesConstantVector` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** `Fill(3.5f, 3)` on GPU.
- **Then** Three elements all 3.5.

#### `Arange_StaticArray_ReturnsExpectedSequence` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** CPU `Arange(0, 4, 1)`.
- **Then** Four values `[0,1,2,3]`.

#### `Arange_NegativeRangeWithPositiveStep_AdjustsStep` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** CPU `Arange(0, -4, 1)`.
- **Then** Four values `[0,-1,-2,-3]`.

#### `Arange_OnGpu_CreatesVector` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** GPU `Arange(2, 8, 2)`.
- **Then** Synced `[2, 4, 6]`.

#### `Linspace_StaticArray_ReturnsEvenlySpacedValues` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** CPU `Linspace(0, 10, 5)`.
- **Then** Five points from 0 to 10 inclusive.

#### `Linspace_OnGpu_CreatesVector` — `VectorFactoriesTests`

- **Source:** `Tests/Core/Vector/VectorFactoriesTests.cs` · `[Fact]`
- **Given** GPU `Linspace(0, 4, 3)`.
- **Then** Synced `[0, 2, 4]`.

### `VectorIndexingTests`

#### `Indexer1D_ReturnsCorrectValue` — `VectorIndexingTests`

- **Source:** `Tests/Core/Vector/VectorIndexingTests.cs` · `[Theory]`
- **Given** Small 1D fixture `[1..5]`.
- **Then** `vector[index]` equals expected.
- **Cases:**
  - index=0 → 1
  - index=2 → 3
  - index=4 → 5

#### `GetAt2D_ReturnsCorrectValue` — `VectorIndexingTests`

- **Source:** `Tests/Core/Vector/VectorIndexingTests.cs` · `[Theory]`
- **Given** 3×5 matrix fixture.
- **Then** `GetAt(row,col)` equals expected.
- **Cases:**
  - (0,0)→1
  - (1,2)→8
  - (2,4)→15

#### `SetAt_UpdatesValue` — `VectorIndexingTests`

- **Source:** `Tests/Core/Vector/VectorIndexingTests.cs` · `[Fact]`
- **Given** Copy of small 1D; index 2 set to 99 via CPU scope.
- **Then** After sync, `Value[2]` is 99.

#### `GetSliceAsArray_Column_ReturnsColumnValues` — `VectorIndexingTests`

- **Source:** `Tests/Core/Vector/VectorIndexingTests.cs` · `[Fact]`
- **Given** 3×5 matrix; column 1.
- **Then** `GetSliceAsArrayX` returns `[2,7,12]`.

#### `GetSliceAsArray_Row_ReturnsRowValues` — `VectorIndexingTests`

- **Source:** `Tests/Core/Vector/VectorIndexingTests.cs` · `[Fact]`
- **Given** 3×5 matrix; row 0.
- **Then** Row slice `[1,2,3,4,5]`.

### `VectorInPlaceTests`

#### `Abs_IP_MutatesInPlace` — `VectorInPlaceTests`

- **Source:** `Tests/Core/Vector/VectorInPlaceTests.cs` · `[Fact]`
- **Given** Mixed-sign fixture copy.
- **Then** `AbsIP` then sync matches CPU abs reference.

#### `IPOP_Add_MutatesInPlace` — `VectorInPlaceTests`

- **Source:** `Tests/Core/Vector/VectorInPlaceTests.cs` · `[Fact]`
- **Given** `[1,2,3]` plus `[10,20,30]`.
- **Then** Result `[11,22,33]` on CPU.

#### `IPOP_ScalarMultiply_MutatesInPlace` — `VectorInPlaceTests`

- **Source:** `Tests/Core/Vector/VectorInPlaceTests.cs` · `[Fact]`
- **Given** `[2,4,6]` times 3.
- **Then** Result `[6,12,18]`.

#### `Normalise_IP_DividesBySum` — `VectorInPlaceTests`

- **Source:** `Tests/Core/Vector/VectorInPlaceTests.cs` · `[Fact]`
- **Given** `[2,2,2]` normalized on GPU.
- **Then** `Sum()` close to 1 within `1e-3`.

### `VectorMatrixOpsTests`

#### `MatrixBinary_SameShape_MatchesElementWise` — `VectorMatrixOpsTests`

- **Source:** `Tests/Core/Vector/VectorMatrixOpsTests.cs` · `[Theory]`
- **Given** Equal 2×3 matrices; `op` is add/subtract/divide/pow.
- **Then** `MatrixAddX` / `MatrixSubtractX` / `MatrixDivideX` / `MatrixPowX` matches NumPy element-wise reference.
- **Cases:**
  - op=add
  - op=subtract
  - op=divide
  - op=pow

#### `MatrixAdd_DifferentShapes_ThrowsShapeMismatch` — `VectorMatrixOpsTests`

- **Source:** `Tests/Core/Vector/VectorMatrixOpsTests.cs` · `[Fact]`
- **Given** 2×3 vs 3×2 matrices.
- **Then** `MatrixAddX` throws `ShapeMismatchException`.

#### `MatrixMultiply_AliasMatchesCross` — `VectorMatrixOpsTests`

- **Source:** `Tests/Core/Vector/VectorMatrixOpsTests.cs` · `[Fact]`
- **Given** 2×3 times 3×2 matmul.
- **Then** `CrossX` and `MatrixMultiplyX` match same reference.

#### `MatrixAdd_On1DVector_ThrowsShapeMismatch` — `VectorMatrixOpsTests`

- **Source:** `Tests/Core/Vector/VectorMatrixOpsTests.cs` · `[Fact]`
- **Given** Length-3 vector vs 2×3 matrix.
- **Then** `MatrixAddX` throws `ShapeMismatchException`.

### `VectorReduceTests`

#### `ReduceOPX_1DWithMatrix_MatchesRowReduction` — `VectorReduceTests`

- **Source:** `Tests/Core/Vector/VectorReduceTests.cs` · `[Theory]`
- **Given** Length-3 coefficient vector and 2×3 matrix; `op` add or multiply.
- **Then** `ReduceOPX` yields length-2 column (`Columns` 1) matching per-row accumulation reference.
- **Cases:**
  - op=add
  - op=multiply

#### `ReduceOPX_UnequalCoefficientLength_ThrowsLengthMismatch` — `VectorReduceTests`

- **Source:** `Tests/Core/Vector/VectorReduceTests.cs` · `[Fact]`
- **Given** Length-3 coeffs vs 2×4 matrix.
- **Then** Throws `LengthMismatchException`.

#### `ReduceOPX_MatrixOnLeft_ThrowsShapeMismatch` — `VectorReduceTests`

- **Source:** `Tests/Core/Vector/VectorReduceTests.cs` · `[Fact]`
- **Given** Matrix left operand.
- **Then** `matrix.ReduceOPX(vector)` throws `ShapeMismatchException`.

#### `Operator_NoLongerUsesRowReduction` — `VectorReduceTests`

- **Source:** `Tests/Core/Vector/VectorReduceTests.cs` · `[Fact]`
- **Given** Length-3 coeff and 2×3 matrix.
- **Then** `coeff + matrix` broadcasts to length 6; `ReduceOPX` still returns length 2.

### `VectorScalarBroadcastTests`

#### `ScalarOp1D_MatchesBroadcastReference` — `VectorScalarBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorScalarBroadcastTests.cs` · `[Theory]`
- **Given** Length-4 vector and scalar.
- **Then** Binary ops with scalar on left or right match NumPy reference shapes.
- **Cases:**
  - scalar=2
  - scalar=-1.5
  - scalar=0.5

#### `ScalarOp2D_MatchesBroadcastReference` — `VectorScalarBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorScalarBroadcastTests.cs` · `[Theory]`
- **Given** 2×3 matrix and scalar.
- **Then** Add/multiply/divide and `scalar - matrix` match reference.
- **Cases:**
  - scalar=2
  - scalar=3

#### `IPOP_ScalarOn2D_MutatesAllElements` — `VectorScalarBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorScalarBroadcastTests.cs` · `[Fact]`
- **Given** 2×3 matrix; add 2 in place.
- **Then** Values match add-scalar reference.

#### `IPOP_ScalarMultiplyOn1D_MutatesInPlace` — `VectorScalarBroadcastTests`

- **Source:** `Tests/Core/Vector/VectorScalarBroadcastTests.cs` · `[Fact]`
- **Given** Length-3 vector; multiply by 2.
- **Then** Matches scale reference.

### `VectorSliceTests`

#### `GetColumnAsArray_ReturnsColumn` — `VectorSliceTests`

- **Source:** `Tests/Core/Vector/VectorSliceTests.cs` · `[Fact]`
- **Given** Shared 3×5 matrix; column 0.
- **Then** `[1,6,11]`.

#### `GetRowAsArray_ReturnsRow` — `VectorSliceTests`

- **Source:** `Tests/Core/Vector/VectorSliceTests.cs` · `[Fact]`
- **Given** Row 1.
- **Then** `[6,7,8,9,10]`.

#### `GetColumnAsVector_ReturnsVector` — `VectorSliceTests`

- **Source:** `Tests/Core/Vector/VectorSliceTests.cs` · `[Fact]`
- **Given** Column 2 as vector.
- **Then** Length 3; values `[3,8,13]`.

#### `GetRowAsVector_ReturnsVector` — `VectorSliceTests`

- **Source:** `Tests/Core/Vector/VectorSliceTests.cs` · `[Fact]`
- **Given** Row 2.
- **Then** Length 5; values `[11..15]`.

#### `GetSliceAsVector_Column_ReturnsSlice` — `VectorSliceTests`

- **Source:** `Tests/Core/Vector/VectorSliceTests.cs` · `[Fact]`
- **Given** Column slice index 3.
- **Then** `[4,9,14]`.

### `VectorStatsTests`

#### `Sum_ReturnsTotal` — `VectorStatsTests`

- **Source:** `Tests/Core/Vector/VectorStatsTests.cs` · `[Fact]`
- **Given** Small 1D `[1..5]`.
- **Then** `Sum()` is 15.

#### `Mean_ReturnsAverage` — `VectorStatsTests`

- **Source:** `Tests/Core/Vector/VectorStatsTests.cs` · `[Fact]`
- **Given** Small 1D.
- **Then** `Mean()` is 3.

#### `MinMaxRange_ReturnCorrectValues` — `VectorStatsTests`

- **Source:** `Tests/Core/Vector/VectorStatsTests.cs` · `[Fact]`
- **Given** Mixed-sign fixture.
- **Then** Min -4, max 5, range 9.

#### `VarAndStd_MatchExpectedForSimpleDataset` — `VectorStatsTests`

- **Source:** `Tests/Core/Vector/VectorStatsTests.cs` · `[Fact]`
- **Given** Eight-value dataset `[2,4,4,4,5,5,7,9]`.
- **Then** Mean 5; population var/std ≈ 4 and 2; sample var/std ≈ 32/7 and sqrt(32/7) within `1e-3`.

#### `Sum_LargeVector_UsesKahanSummation` — `VectorStatsTests`

- **Source:** `Tests/Core/Vector/VectorStatsTests.cs` · `[Fact]`
- **Given** 10_000 elements of 0.1.
- **Then** `Sum()` close to 1000 within tolerance 1.

### `VectorStructuralTests`

#### `Transpose_SwapsRowsAndColumns` — `VectorStructuralTests`

- **Source:** `Tests/Core/Vector/VectorStructuralTests.cs` · `[Fact]`
- **Given** 3×5 matrix.
- **Then** Transpose has 5 rows, 3 columns; values match CPU transpose.

#### `Dot_ReturnsScalarProduct` — `VectorStructuralTests`

- **Source:** `Tests/Core/Vector/VectorStructuralTests.cs` · `[Fact]`
- **Given** `[1,2,3]` dot `[4,5,6]`.
- **Then** Scalar 32.

#### `Concat_RowAxis_AppendsViaAppend` — `VectorStructuralTests`

- **Source:** `Tests/Core/Vector/VectorStructuralTests.cs` · `[Fact]`
- **Given** Two length-3 vectors.
- **Then** Row concat length 6 with values `[1..6]` after sync.

#### `Append_JoinsVectors` — `VectorStructuralTests`

- **Source:** `Tests/Core/Vector/VectorStructuralTests.cs` · `[Fact]`
- **Given** `[1,2]` and `[3,4]`.
- **Then** `[1,2,3,4]`.

#### `Prepend_PutsSecondVectorFirst` — `VectorStructuralTests`

- **Source:** `Tests/Core/Vector/VectorStructuralTests.cs` · `[Fact]`
- **Given** `[1,2]` and `[3,4]`.
- **Then** `[3,4,1,2]`.

#### `Merge_RemovesDuplicates` — `VectorStructuralTests`

- **Source:** `Tests/Core/Vector/VectorStructuralTests.cs` · `[Fact]`
- **Given** `[1,2,3]` merge `[3,4,5]`.
- **Then** Length 5 containing 1–5.

### `VectorSyncCoherenceTests`

#### `UpdateCache_AfterCpuResize_DoesNotShrinkValue` — `VectorSyncCoherenceTests`

- **Source:** `Tests/Core/Vector/VectorSyncCoherenceTests.cs` · `[Fact]`
- **Given** Vector grown on CPU from 3 to 6 elements while GPU buffer existed.
- **Then** After `UpdateCache`, length 6 and CPU array `[1..6]`.

#### `Append_IP_PreservesValueAfterGpuCache` — `VectorSyncCoherenceTests`

- **Source:** `Tests/Core/Vector/VectorSyncCoherenceTests.cs` · `[Fact]`
- **Given** Append `[4,5,6]` to `[1,2,3]`.
- **Then** Length 6; `ToArray` is `[1..6]`.

#### `UpdateCache_InSync_IsNoOpOnSecondCall` — `VectorSyncCoherenceTests`

- **Source:** `Tests/Core/Vector/VectorSyncCoherenceTests.cs` · `[Fact]`
- **Given** In-sync vector.
- **Then** Second `UpdateCache` keeps same buffer id and values.

#### `UpdateCache_LruResizeViaCpuAuthority_ReallocatesGpuBuffer` — `VectorSyncCoherenceTests`

- **Source:** `Tests/Core/Vector/VectorSyncCoherenceTests.cs` · `[Fact]`
- **Given** Vector on LRU GPU; CPU authority expands to four elements.
- **Then** `Pull` and `ToArray` match `[10,20,30,40]`.

#### `SyncCPU_InSync_IsNoOp` — `VectorSyncCoherenceTests`

- **Source:** `Tests/Core/Vector/VectorSyncCoherenceTests.cs` · `[Fact]`
- **Given** `Residence.InSync`.
- **Then** `SyncCPU` leaves `InSync`.

### `VectorTransferTests`

#### `TransferBuffer_MovesGpuBufferToTarget` — `VectorTransferTests`

- **Source:** `Tests/Core/Vector/VectorTransferTests.cs` · `[Fact]`
- **Given** Target `[1,2,3]` and donor `[9,8,7]`.
- **Then** Target adopts donor GPU id; donor id 0; target in sync with `[9,8,7]`; donor CPU values unchanged.

### `VectorUnaryTests`

#### `Abs_CpuPath_ReturnsAbsoluteValues` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** Mixed-sign fixture.
- **Then** `Abs()` matches CPU abs.

#### `AbsX_GpuPath_MatchesAbs` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** Same fixture.
- **Then** GPU `AbsX` matches CPU `Abs`.

#### `Reverse_CpuPath_ReversesOrder` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** Small 1D.
- **Then** Reversed order matches CPU reference.

#### `ReverseX_GpuPath_MatchesReverse` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** Small 1D.
- **Then** GPU reverse matches CPU.

#### `Reciprocal_ReturnsOneOverValues` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** `[2,4,5]`.
- **Then** `[0.5, 0.25, 0.2]`.

#### `Rsqrt_PositiveValues_ReturnReciprocalSqrt` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Theory]`
- **Given** Single-element positive input.
- **Then** `Rsqrt` within `1e-3` of expected reciprocal square root.
- **Cases:**
  - input=4 → 0.5
  - input=9 → 1/3
  - input=16 → 0.25

#### `Rsqrt_Zero_ReturnsPositiveInfinity` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** `[0]`.
- **Then** Positive infinity.

#### `Rsqrt_NegativeValue_ReturnsNaN` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** `[-4]`.
- **Then** NaN.

#### `Rsqrt_MixedSigns_ComputesPerElement` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** `[4, 0, -9, 16]`.
- **Then** 0.5, +inf, NaN, 0.25 respectively.

#### `RsqrtX_GpuPath_MatchesRsqrt` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** Same mixed vector.
- **Then** GPU matches CPU `Rsqrt`.

#### `Diff_ReturnsAdjacentDifferences` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** `[1,3,6,10]`.
- **Then** `DiffX` → `[2,3,4]`.

#### `Nan_to_num_ReplacesNonFiniteValues` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** NaN/Inf fixture with fill -1.
- **Then** All lanes -1 except 0 stays 0.

#### `Log_IP_ComputesLogWithBase` — `VectorUnaryTests`

- **Source:** `Tests/Core/Vector/VectorUnaryTests.cs` · `[Fact]`
- **Given** `[1,10,100]` log base 10 in place.
- **Then** `[0,1,2]` within `1e-3`.

## Core / VectorBase

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `VectorBaseLifecycleTests`

#### `SyncCPU_RoundTripsGpuValues` — `VectorBaseLifecycleTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseLifecycleTests.cs` · `[Fact]`
- **Given** Edit index 1 to 99 via CPU scope, then `SyncCPU`.
- **Then** `Value[1]` is 99.

#### `Pull_ReturnsGpuValues` — `VectorBaseLifecycleTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseLifecycleTests.cs` · `[Fact]`
- **Given** `[4,5,6]`.
- **Then** `Pull()` close to same.

#### `ToArray_ReturnsCpuCopy` — `VectorBaseLifecycleTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseLifecycleTests.cs` · `[Fact]`
- **Given** `[7,8,9]`.
- **Then** `ToArray` matches.

#### `UpdateCache_UpdatesGpuFromCpu` — `VectorBaseLifecycleTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseLifecycleTests.cs` · `[Fact]`
- **Given** Mutate `Value[0]` to 100 on CPU.
- **Then** After update cache + sync, still 100.

#### `DeCache_RemovesFromGpu` — `VectorBaseLifecycleTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseLifecycleTests.cs` · `[Fact]`
- **Given** Cached vector.
- **Then** `ID` 0; CPU values preserved.

### `VectorBaseLiveCountTests`

#### `GpuScope_TracksLiveCountOnModifiedPin` — `VectorBaseLiveCountTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseLiveCountTests.cs` · `[Fact]`
- **Given** Vector in `GpuScope`.
- **Then** LiveCount 1 inside, 0 after.

#### `GpuScopeReadOnly_TracksLiveCountWithoutActiveGpu` — `VectorBaseLiveCountTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseLiveCountTests.cs` · `[Fact]`
- **Given** Read-only GPU scope.
- **Then** LiveCount 1 inside, 0 after.

### `VectorBaseOutputTests`

#### `ToCSV_1D_IncludesCommaSeparatedValues` — `VectorBaseOutputTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseOutputTests.cs` · `[Fact]`
- **Given** `[1,2,3]`.
- **Then** CSV string contains `1,`, `2,`, `3,`.

#### `ToStr_ReturnsFormattedString` — `VectorBaseOutputTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseOutputTests.cs` · `[Fact]`
- **Given** `[1,2,3]`.
- **Then** String representation contains `1`.

### `VectorBaseShapeTests`

#### `Is1D_ReturnsTrueForRow1D` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** Flat `[1,2,3]`.
- **Then** `Is1D()` true.

#### `Is1D_ReturnsTrueForColumnVector` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×1 column.
- **Then** `Is1D()` true.

#### `Is1DRowVector_ReturnsFalseForColumnVector` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×1 column.
- **Then** `Is1DRowVector()` false.

#### `Is1DRowVector_ReturnsTrueForRow1D` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** Flat 1D.
- **Then** `Is1DRowVector()` true.

#### `Is2D_ReturnsTrueForMatrix` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×5 matrix.
- **Then** `Is2D()` true.

#### `Is2D_ReturnsFalseForColumnVector` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×1.
- **Then** `Is2D()` false.

#### `ElementsPerRow_ReturnsLengthForFlat1D` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** Length 3.
- **Then** `ElementsPerRow()` 3.

#### `ElementsPerRow_ReturnsColumnsForMatrix` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×5.
- **Then** `ElementsPerRow()` 5.

#### `Shape_ColumnVector_ReturnsLengthByOne` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×1.
- **Then** `Shape()` is (3,1).

#### `IsRectangular_ReturnsTrueForValidMatrix` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×5.
- **Then** Rectangular.

#### `Shape_Flat1D_ReturnsOneByLength` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** Length 3 flat.
- **Then** `Shape()` (1,3).

#### `RowCount_ReturnsOneFor1D` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** Flat 1D.
- **Then** `RowCount()` 1.

#### `RowCount_ReturnsRowsForMatrix` — `VectorBaseShapeTests`

- **Source:** `Tests/Core/VectorBase/VectorBaseShapeTests.cs` · `[Fact]`
- **Given** 3×5.
- **Then** `RowCount()` 3.

## Core / VectorInt

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `VectorIntBroadcastErrorTests`

#### `Operator_IncompatibleShapes_Throws` — `VectorIntBroadcastErrorTests`

- **Source:** `Tests/Core/VectorInt/VectorIntBroadcastErrorTests.cs` · `[Theory]`
- **Given** Integer tensors with same incompatible shapes as float broadcast tests.
- **Then** `a + b` throws `ShapeMismatchException`.
- **Cases:**
  - shapeA=[3,5] vs shapeB=[2,5] (row count mismatch)
  - shapeA=[3,5] vs shapeB=[3,4] (column mismatch)
  - shapeA=[2,3] vs shapeB=[6] (length-6 vector vs 2×3 matrix)
  - shapeA=[4] vs shapeB=[2,3] (length-4 vs 2×3)
  - shapeA=[2,2] vs shapeB=[3,3] (both matrices, incompatible)

#### `IPOP_Incompatible2DShapes_Throws` — `VectorIntBroadcastErrorTests`

- **Source:** `Tests/Core/VectorInt/VectorIntBroadcastErrorTests.cs` · `[Fact]`
- **Given** 2×3 vs 3×2 integer matrices.
- **Then** `IPOP(add)` throws `ShapeMismatchException`.

### `VectorIntBroadcastTests`

#### `Broadcast_RowVectorPlusMatrix_MatchesReference` — `VectorIntBroadcastTests`

- **Source:** `Tests/Core/VectorInt/VectorIntBroadcastTests.cs` · `[Fact]`
- **Given** 2×3 matrix + 1×3 row.
- **Then** Both orders match integer broadcast add reference.

#### `Broadcast_ColumnPlusMatrix_MatchesReference` — `VectorIntBroadcastTests`

- **Source:** `Tests/Core/VectorInt/VectorIntBroadcastTests.cs` · `[Fact]`
- **Given** 2×3 + 2×1 column.
- **Then** Matches reference.

#### `EqualShape2D_ElementWise_MatchesReference` — `VectorIntBroadcastTests`

- **Source:** `Tests/Core/VectorInt/VectorIntBroadcastTests.cs` · `[Fact]`
- **Given** Equal 2×3 matrices.
- **Then** Element-wise add matches reference.

#### `VectorScalarOperators_MatchReference` — `VectorIntBroadcastTests`

- **Source:** `Tests/Core/VectorInt/VectorIntBroadcastTests.cs` · `[Theory]`
- **Given** `[2,4,6]` and integer scalar.
- **Then** Add, multiply, scalar-minus-vector, and scalar-divide-vector match C# integer semantics.
- **Cases:**
  - scalar=2
  - scalar=-3

#### `Broadcast_1DPlusMatrix_Requires2DStorage` — `VectorIntBroadcastTests`

- **Source:** `Tests/Core/VectorInt/VectorIntBroadcastTests.cs` · `[Fact]`
- **Given** 2×3 matrix and length-3 1D vector.
- **Then** `matrix + vec` throws `ArgumentException` requiring 2D storage layout.

### `VectorIntCompareTests`

#### `VectorCompare_OperatorsAndMethods` — `VectorIntCompareTests`

- **Source:** `Tests/Core/VectorInt/VectorIntCompareTests.cs` · `[Fact]`
- **Given** `[1,2,3,4]` vs `[0,2,5,4]`.
- **Then** Ordering masks and `CompareEqualsX` / `CompareNotEqualsX` match expected bit patterns.

#### `VectorScalarCompare` — `VectorIntCompareTests`

- **Source:** `Tests/Core/VectorInt/VectorIntCompareTests.cs` · `[Fact]`
- **Given** `[0,1,2]` compared to scalar 1.
- **Then** `>`, `>=`, `<`, `<=`, and `CompareEqualsX(1)` masks as expected.

### `VectorIntEdgeCaseTests`

#### `Multiply_Overflow_WrapsUnchecked` — `VectorIntEdgeCaseTests`

- **Source:** `Tests/Core/VectorInt/VectorIntEdgeCaseTests.cs` · `[Fact]`
- **Given** `int.MaxValue`×2 and 1×1.
- **Then** Unchecked wrap on overflow lane.

#### `Divide_ByZero_DoesNotThrowOnSync` — `VectorIntEdgeCaseTests`

- **Source:** `Tests/Core/VectorInt/VectorIntEdgeCaseTests.cs` · `[Fact]`
- **Given** Divide by zero lanes.
- **Then** Syncing result to CPU does not throw.

#### `ExplicitCast_ToVectorInt_RejectsInfinity` — `VectorIntEdgeCaseTests`

- **Source:** `Tests/Core/VectorInt/VectorIntEdgeCaseTests.cs` · `[Fact]`
- **Given** Float vector with infinity.
- **Then** Cast to `VectorInt` throws `InvalidCastException`.

### `VectorIntInPlaceTests`

#### `IPOP_Broadcast_MatchesReference` — `VectorIntInPlaceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntInPlaceTests.cs` · `[Fact]`
- **Given** 2×3 matrix += 1×3 row in place.
- **Then** CPU values equal broadcast add reference.

#### `IPOP_ScalarDivide_MutatesInPlace` — `VectorIntInPlaceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntInPlaceTests.cs` · `[Fact]`
- **Given** `[10,20,30]` divided by 10.
- **Then** `[1,2,3]`.

### `VectorIntIndexingTests`

#### `Indexer1D_ReturnsCorrectValue` — `VectorIntIndexingTests`

- **Source:** `Tests/Core/VectorInt/VectorIntIndexingTests.cs` · `[Theory]`
- **Given** Small int fixture `[1..5]`.
- **Then** Indexer returns expected int.
- **Cases:**
  - 0→1
  - 2→3
  - 4→5

#### `GetAt2D_ReturnsCorrectValue` — `VectorIntIndexingTests`

- **Source:** `Tests/Core/VectorInt/VectorIntIndexingTests.cs` · `[Theory]`
- **Given** 3×5 int matrix.
- **Then** `GetAt` matches matrix entries.
- **Cases:**
  - (0,0)→1
  - (1,2)→8
  - (2,4)→15

#### `SetAt_UpdatesValue` — `VectorIntIndexingTests`

- **Source:** `Tests/Core/VectorInt/VectorIntIndexingTests.cs` · `[Fact]`
- **Given** CPU scope sets index 2 to 99.
- **Then** `Value[2]` is 99 after sync.

#### `GetSliceAsArray_Column_ReturnsColumnValues` — `VectorIntIndexingTests`

- **Source:** `Tests/Core/VectorInt/VectorIntIndexingTests.cs` · `[Fact]`
- **Given** Column 1.
- **Then** `[2,7,12]`.

#### `GetSliceAsArray_Row_ReturnsRowValues` — `VectorIntIndexingTests`

- **Source:** `Tests/Core/VectorInt/VectorIntIndexingTests.cs` · `[Fact]`
- **Given** Row 0.
- **Then** `[1,2,3,4,5]`.

### `VectorIntReduceTests`

#### `ReduceOPX_1DWithMatrix_MatchesRowReduction` — `VectorIntReduceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntReduceTests.cs` · `[Theory]`
- **Given** Coeffs `[1,1,1]` and 2×3 matrix `[10..15]`; add or multiply op.
- **Then** Length-2 column result matches row-reduce reference.
- **Cases:**
  - op=add
  - op=multiply

#### `ReduceOPX_UnequalCoefficientLength_ThrowsLengthMismatch` — `VectorIntReduceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntReduceTests.cs` · `[Fact]`
- **Given** Length-3 vs 2×4.
- **Then** `LengthMismatchException`.

#### `ReduceOPX_MatrixOnLeft_ThrowsShapeMismatch` — `VectorIntReduceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntReduceTests.cs` · `[Fact]`
- **Given** Matrix on left.
- **Then** `ShapeMismatchException`.

### `VectorIntSliceTests`

#### `GetColumnAsArray_ReturnsColumn` — `VectorIntSliceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntSliceTests.cs` · `[Fact]`
- **Given** 3×5 matrix column 0.
- **Then** `[1,6,11]`.

#### `GetRowAsArray_ReturnsRow` — `VectorIntSliceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntSliceTests.cs` · `[Fact]`
- **Given** Row 1.
- **Then** `[6,7,8,9,10]`.

#### `GetColumnAsVector_ReturnsVector` — `VectorIntSliceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntSliceTests.cs` · `[Fact]`
- **Given** Column 2.
- **Then** Length 3; `[3,8,13]`.

#### `GetRowAsVector_ReturnsRow` — `VectorIntSliceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntSliceTests.cs` · `[Fact]`
- **Given** Row 2.
- **Then** `[11..15]`.

#### `GetSliceAsVector_Column_ReturnsSlice` — `VectorIntSliceTests`

- **Source:** `Tests/Core/VectorInt/VectorIntSliceTests.cs` · `[Fact]`
- **Given** Column index 3.
- **Then** `[4,9,14]`.

### `VectorIntStatsTests`

#### `VarAndStd_MatchExpectedForSimpleDataset` — `VectorIntStatsTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStatsTests.cs` · `[Fact]`
- **Given** `[2,4,4,4,5,5,7,9]`.
- **Then** Same mean/var/std expectations as float stats test within `1e-3`.

#### `Linspace_OnGpu_CreatesVector` — `VectorIntStatsTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStatsTests.cs` · `[Fact]`
- **Given** `Linspace(0,10,5)` on GPU.
- **Then** `[0,2,4,6,8]`.

### `VectorIntStructuralTests`

#### `Transpose_SwapsRowsAndColumns` — `VectorIntStructuralTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStructuralTests.cs` · `[Fact]`
- **Given** 3×5 int matrix.
- **Then** 5×3 layout with transposed values.

#### `Concat_RowAxis_AppendsViaAppend` — `VectorIntStructuralTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStructuralTests.cs` · `[Fact]`
- **Given** Two length-3 vectors.
- **Then** Length 6 row concat.

#### `Append_JoinsVectors` — `VectorIntStructuralTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStructuralTests.cs` · `[Fact]`
- **Given** `[1,2]` + `[3,4]`.
- **Then** `[1,2,3,4]`.

#### `Prepend_PutsSecondVectorFirst` — `VectorIntStructuralTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStructuralTests.cs` · `[Fact]`
- **Given** Same pair.
- **Then** `[3,4,1,2]`.

#### `Merge_RemovesDuplicates` — `VectorIntStructuralTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStructuralTests.cs` · `[Fact]`
- **Given** `[1,2,3]` and `[3,4,5]`.
- **Then** Length 5 with 1–5.

#### `Flatten_SetsColumnsToZero` — `VectorIntStructuralTests`

- **Source:** `Tests/Core/VectorInt/VectorIntStructuralTests.cs` · `[Fact]`
- **Given** 2×2 matrix storage.
- **Then** `Columns` 0; length 4.

### `VectorIntTests`

#### `Constructor_WithValues_SetsDimensions` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Five-element int fixture.
- **Then** Length 5, columns 0, values match.

#### `Copy_CreatesIndependentVector` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Small fixture.
- **Then** Equal but not same reference.

#### `Equals_ReturnsFalseForDifferentValues` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[1,2,3]` vs `[1,2,4]`.
- **Then** Not equal.

#### `Operators_VectorVector_MatchExpected` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Theory]`
- **Given** Length-3 int pairs and `op` add/subtract/multiply.
- **Then** Element-wise result matches expected triple.
- **Cases:**
  - add: [1,2,3]+[10,20,30]→[11,22,33]
  - subtract: [10,20,30]-[1,2,3]→[9,18,27]
  - multiply: [2,4,6]*[3,3,3]→[6,12,18]

#### `Divide_TruncatesTowardZero` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[7,-7,8]` / `[2,2,-3]`.
- **Then** `[3,-3,-2]`.

#### `UnsupportedOperation_ThrowsForInvalidReduceOp` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Reduce with `bitwiseXor`.
- **Then** `UnsupportedOperationException`.

#### `UnsupportedOperation_ThrowsForUnknownElementWiseOp` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `OP(..., pow)`.
- **Then** `InvalidOperationOnTypeException`.

#### `Modulo_MatchesCSharpSemantics` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Vector and scalar `%` cases.
- **Then** Matches C# `%` for each lane.

#### `Xor_ScalarAndVector` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Bitwise XOR vector and scalar.
- **Then** `[2,3,1]` and `[0,3,2]` patterns.

#### `BitwiseAnd_ScalarAndVector` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `&` vector and scalar.
- **Then** Expected AND lanes.

#### `IPOP_Add_MutatesInPlace` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `+=` another vector.
- **Then** `[11,22,33]`.

#### `IPOP_ScalarMultiply_MutatesInPlace` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `*= 3`.
- **Then** `[6,12,18]`.

#### `Broadcast_RowVectorPlusScalar_MatchesExpected` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** 2×2 stored as length 4.
- **Then** `+10` → `[11,12,13,14]`.

#### `LeftShift_Scalar_ShiftsEveryElement` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `<< 2` on `[1,2,3]`.
- **Then** `[4,8,12]`.

#### `RightShift_Scalar_IsArithmeticSignExtending` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[8,-8] >> 1`.
- **Then** `[4,-4]`.

#### `LeftShift_VectorCounts_Broadcasts` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Ones shifted by `[1,2,3]`.
- **Then** `[2,4,8]`.

#### `Shift_EdgeCounts_MatchCSharpMasking` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Shift counts 0, 31, 32.
- **Then** Matches C# shift masking per lane.

#### `InPlaceShift_MutatesInPlace` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `<<=` then `>>=` on `[1,2,3]`.
- **Then** Returns to `[1,2,3]`.

#### `Compare_ProducesExpectedMask` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[1,2,3,4]` vs `[0,2,5,4]`.
- **Then** `>` and `CompareEqualsX` masks as in test.

#### `Mask_AndOperator_AppliesFillValue` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Mask with false lanes.
- **Then** AND fills false with 0 or custom -1.

#### `Filter_CompactsToSurvivingLanes` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Filter mask on `[10,20,30,40]`.
- **Then** `|`, `FilterX`, indexer → `[10,30]`.

#### `Partition_SplitsTrueAndFalseLanes` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `/` mask operator.
- **Then** True lanes `[1,3]`, false `[2,4]`.

#### `ExplicitCast_ToVector_ConvertsElementsToFloat` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Int to float cast.
- **Then** Close to `[1,-2,3]`.

#### `ExplicitCast_ToVectorInt_TruncatesTowardZero` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Float `[1.9,-1.9,2.1]`.
- **Then** `[1,-1,2]`.

#### `ExplicitCast_ToVectorInt_RejectsNonFinite` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Float with NaN.
- **Then** `InvalidCastException`.

#### `Sum_And_Mean_ComputeExpectedValues` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[1,2,3,4]`.
- **Then** Sum 10, mean 2.5.

#### `MinMaxRange_All_ComputeExpectedValues` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[5,-3,10,2]` and zeros.
- **Then** Min/max/range; `All` false when zeros present.

#### `Abs_ComputesBitwiseAbsoluteValue` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Includes `int.MinValue`.
- **Then** Bitwise abs and unary `+` agree.

#### `Negate_HandlesMinValueWithoutException` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `int.MinValue` lane.
- **Then** Negation matches C# rules.

#### `AbsX_GpuKernel_MatchesCpuAbs` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Negative int vector.
- **Then** GPU abs matches CPU bitwise abs.

#### `Diff_ComputesConsecutiveDifferences` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[1,3,6,10]`.
- **Then** `[2,3,4]`.

#### `Dot_ComputesSumOfProducts` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[1,2,3]`·`[4,5,6]`.
- **Then** 32.

#### `InvalidOperationOnType_ThrowsForFloatOnlyOp` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `distance` op on ints.
- **Then** `InvalidOperationOnTypeException`.

#### `Factories_ZerosOnesFillArange_ProduceExpectedValues` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Static factories on GPU.
- **Then** Zeros, ones, fill 7, arange 0..4.

#### `MatrixMultiply_ComputesExpectedProduct` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** 2×2 matrices.
- **Then** `[19,22,43,50]`.

#### `Reverse_ReversesElementOrder` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** `[1,2,3,4]`.
- **Then** Reversed.

#### `ReverseX_GpuKernel_ReversesElementOrder` — `VectorIntTests`

- **Source:** `Tests/Core/VectorInt/VectorIntTests.cs` · `[Fact]`
- **Given** Same vector.
- **Then** GPU reverse matches.

## Experimental

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `TestClsAccuracyTests`

#### `Sqrt_ApproximatesMathSqrt` — `TestClsAccuracyTests`

- **Source:** `Tests/Experimental/TestClsAccuracyTests.cs` · `[Theory]`
- **Given** A positive float `input` passed to experimental `TestCls.Sqrt`.
- **Then** The result is within tolerance `0.01` of `MathF.Sqrt(input)`.
- **Cases:**
  - input=1f
  - input=4f
  - input=9f
  - input=16f
  - input=2f

#### `CBRT_ApproximatesMathCbrt` — `TestClsAccuracyTests`

- **Source:** `Tests/Experimental/TestClsAccuracyTests.cs` · `[Theory]`
- **Given** A non-negative double `input` passed to `TestCls.CBRT`.
- **Then** |actual − `Math.Cbrt(input)`| is less than `0.1`.
- **Cases:**
  - input=1f
  - input=8f
  - input=27f

#### `LOG2_ApproximatesMathLog2` — `TestClsAccuracyTests`

- **Source:** `Tests/Experimental/TestClsAccuracyTests.cs` · `[Theory]`
- **Given** A positive double `input` passed to `TestCls.LOG2`.
- **Then** |actual − `Math.Log2(input)`| is less than `0.1`.
- **Cases:**
  - input=1.0
  - input=8.0
  - input=1024.0

#### `LOG10_ApproximatesMathLog10` — `TestClsAccuracyTests`

- **Source:** `Tests/Experimental/TestClsAccuracyTests.cs` · `[Theory]`
- **Given** A positive double `input` passed to `TestCls.LOG10`.
- **Then** |actual − `Math.Log10(input)`| is less than `0.1`.
- **Cases:**
  - input=1.0
  - input=10.0
  - input=100.0

## Extensions

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `ArrayExtensionsTests`

#### `Sum_FloatArray_ReturnsTotal` — `ArrayExtensionsTests`

- **Source:** `Tests/Extensions/ArrayExtensionsTests.cs` · `[Fact]`
- **Given** A `float[]` `{1, 2, 3, 4}`.
- **Then** Extension `Sum()` returns `10f`.

#### `Sum_DoubleArray_ReturnsTotal` — `ArrayExtensionsTests`

- **Source:** `Tests/Extensions/ArrayExtensionsTests.cs` · `[Fact]`
- **Given** A `double[]` `{1, 2, 3, 4}`.
- **Then** Extension `Sum()` returns `10`.

#### `Average_FloatArray_ReturnsMean` — `ArrayExtensionsTests`

- **Source:** `Tests/Extensions/ArrayExtensionsTests.cs` · `[Fact]`
- **Given** A `float[]` `{2, 4, 6}`.
- **Then** Extension `Average()` returns `4f`.

#### `MinMax_FloatArray_ReturnExtremes` — `ArrayExtensionsTests`

- **Source:** `Tests/Extensions/ArrayExtensionsTests.cs` · `[Fact]`
- **Given** A `float[]` `{3, 1, 5, 2}`.
- **Then** `Min()` is `1f` and `Max()` is `5f`.

#### `MinMax_IntArray_ReturnExtremes` — `ArrayExtensionsTests`

- **Source:** `Tests/Extensions/ArrayExtensionsTests.cs` · `[Fact]`
- **Given** An `int[]` `{3, 1, 5, 2}`.
- **Then** `Min()` is `1` and `Max()` is `5`.

### `PrintExtensionsTests`

#### `ToStr_FloatArray_ReturnsBracketedString` — `PrintExtensionsTests`

- **Source:** `Tests/Extensions/PrintExtensionsTests.cs` · `[Fact]`
- **Given** A `float[]` `{1, 2, 3}`.
- **Then** `ToStr()` output contains the substring `"1"`.

#### `Print_Double2D_ThrowsNotImplemented` — `PrintExtensionsTests`

- **Source:** `Tests/Extensions/PrintExtensionsTests.cs` · `[Fact]`
- **Given** A `double[,]` with one row `{1, 2}`.
- **Then** Calling `Print()` throws `NotImplementedException`.

## Geometric

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `VertexTests`

#### `Constructor_SetsComponents` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** `new Vertex(1f, 2f, 3f)`.
- **Then** `X`, `Y`, and `Z` are `1f`, `2f`, and `3f`.

#### `Constructor_InvalidArrayLength_Throws` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** A two-element float array `{1, 2}` passed to the array constructor.
- **Then** Throws `Exception` with a message containing `3 Values`.

#### `PresetVertices_ReturnExpectedValues` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Theory]`
- **Given** A named preset accessor on `Vertex` (`UP`, `DOWN`, or `FORWARD`).
- **Then** `X`, `Y`, and `Z` match the expected unit-axis components.
- **Cases:**
  - name=UP → `(0, 1, 0)`
  - name=DOWN → `(0, -1, 0)`
  - name=FORWARD → `(1, 0, 0)`

#### `Addition_AddsComponents` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** Vertices `(1,2,3)` and `(4,5,6)`.
- **Then** `a + b` has components `(5, 7, 9)`.

#### `Magnitude_ReturnsLength` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** Vertex `(3, 4, 0)`.
- **Then** `Magnitude()` is `5f`.

#### `Distance_ReturnsDistanceBetweenVertices` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** Vertices at the origin and `(3, 4, 0)`.
- **Then** `Distance` between them is `5f`.

#### `Dot_ReturnsScalarProduct` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** Vertices `(1,2,3)` and `(4,5,6)`.
- **Then** `Dot` returns `32f`.

#### `Equals_UsesTolerance` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** Vertices `(1,2,3)` and `(1.000001, 2, 3)`.
- **Then** `Vertex.Equals(a, b)` is true.

#### `UnitVector_NormalizesToLengthOne` — `VertexTests`

- **Source:** `Tests/Geometric/VertexTests.cs` · `[Fact]`
- **Given** Vertex `(3, 4, 0)` after in-place `UnitVectorIP()`.
- **Then** `Magnitude()` is approximately `1f` (test tolerance).

## Geometric / Vector3

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `Vector3ConstructionTests`

#### `Constructor_WithValues_SetsLengthAndColumns` — `Vector3ConstructionTests`

- **Source:** `Tests/Geometric/Vector3/Vector3ConstructionTests.cs` · `[Fact]`
- **Given** Nine floats `{1..9}` (three 3D rows) on the test GPU.
- **Then** `Length` is `9` and `Columns` is `3`.

#### `Constructor_InvalidLength_Throws` — `Vector3ConstructionTests`

- **Source:** `Tests/Geometric/Vector3/Vector3ConstructionTests.cs` · `[Fact]`
- **Given** Two floats `{1, 2}` (not a multiple of three).
- **Then** Throws `Exception` with a message containing `multiple of 3`.

#### `Zeros_CreatesZeroFilledVector3` — `Vector3ConstructionTests`

- **Source:** `Tests/Geometric/Vector3/Vector3ConstructionTests.cs` · `[Fact]`
- **Given** `Zeros` with length `6` on the test GPU.
- **Then** `Length` is `6` and every stored component is `0f`.

#### `Fill_CreatesConstantVector3` — `Vector3ConstructionTests`

- **Source:** `Tests/Geometric/Vector3/Vector3ConstructionTests.cs` · `[Fact]`
- **Given** `Fill` with value `2.5f` and length `6`.
- **Then** Every component is `2.5f`.

### `Vector3ConversionTests`

#### `ToVector_ConvertsToGenericVector` — `Vector3ConversionTests`

- **Source:** `Tests/Geometric/Vector3/Vector3ConversionTests.cs` · `[Fact]`
- **Given** A `Vector3` built from `{1..9}`.
- **Then** `ToVector()` yields length `9`, `Columns` `3`, and CPU values match `{1..9}` within test tolerance.

#### `VectorToVector3_ConvertsWhenLengthValid` — `Vector3ConversionTests`

- **Source:** `Tests/Geometric/Vector3/Vector3ConversionTests.cs` · `[Fact]`
- **Given** A `Vector` containing `{1..9}`.
- **Then** `ToVector3()` CPU values match `{1..9}` within test tolerance.

### `Vector3GeometryTests`

#### `Cross_ProducesPerpendicularVector` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Single-row vectors along +X and +Y.
- **Then** `CrossX` yields approximately `(0, 0, 1)`.

#### `Magnitude_ReturnsLengthPerRow` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** One row `(3, 4, 0)`.
- **Then** `MagnitudeX()[0]` is approximately `5f`.

#### `Distance_ReturnsDistancePerRow` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Rows `(0,0,0)` and `(3,4,0)`.
- **Then** `DistanceX` at row 0 is approximately `5f`.

#### `Dot_OrthogonalVectors_ReturnsZero` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Orthogonal unit-axis rows +X and +Y.
- **Then** `DotX` at row 0 is approximately `0f`.

#### `Dot_SameVector_ReturnsMagnitudeSquared` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** One row `(3, 4, 0)`.
- **Then** `DotX(vec, vec)` at row 0 is approximately `25f`.

#### `Dot_Batch_ReturnsPerRowScalars` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Two-row batch with explicit component arrays in the test.
- **Then** Row 0 dot is approximately `50f` and row 1 dot is approximately `4f`.

#### `Dot_DifferentLengths_ThrowsLengthMismatchException` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** `Vector3` instances with one row vs two rows.
- **Then** `DotX` throws `LengthMismatchException` mentioning `Dot`.

#### `Dot_StaticAndInstance_ReturnEqualResults` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** The same two-row pair used in the batch dot test.
- **Then** `Vector3.DotX(a,b)` matches `a.DotX(b)` element-wise within test tolerance.

#### `Magnitude_DifferentLengths_ThrowsLengthMismatchException` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** One-row and three-row `Vector3` operands.
- **Then** `MagnitudeX(b)` throws `LengthMismatchException` mentioning `Magnitude`.

#### `Distance_DifferentLengths_ThrowsLengthMismatchException` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** One-row and two-row `Vector3` operands.
- **Then** `DistanceX` throws `LengthMismatchException` mentioning `Distance`.

#### `Normalise_SingleVec3_ReturnsUnitVector` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Row `(1, 2, 3)` and `invMagnitude = 1/sqrt(14)`.
- **Then** `NormaliseX` components match `(invMagnitude, 2*invMagnitude, 3*invMagnitude)` within test tolerance.

#### `Normalise_Batch_NormalisesEachRowIndependently` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Four rows of ascending components (12 floats) as in the test.
- **Then** Each row is scaled by its own inverse magnitude; full buffer matches the test’s expected array within tolerance.

#### `Normalise_PostNormaliseMagnitude_IsApproximatelyOne` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** The same four-row batch normalized in place.
- **Then** `MagnitudeX` for every row is approximately `1f`.

#### `Normalise_ZeroVector_ProducesNaN` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Row `(0, 0, 0)`.
- **Then** Every component after `NormaliseX` is NaN.

#### `Normalise_StaticAndInstance_ReturnEqualResults` — `Vector3GeometryTests`

- **Source:** `Tests/Geometric/Vector3/Vector3GeometryTests.cs` · `[Fact]`
- **Given** Two-row data `(3,4,0)` and `(1,1,1)`.
- **Then** Static `Vector3.NormaliseX` matches instance `NormaliseX` within test tolerance.

### `Vector3IndexingTests`

#### `Indexer_SingleVertex_ReturnsCoordValue` — `Vector3IndexingTests`

- **Source:** `Tests/Geometric/Vector3/Vector3IndexingTests.cs` · `[Fact]`
- **Given** One row `(1, 2, 3)`.
- **Then** `vec[0, Coord.x/y/z]` are `1f`, `2f`, `3f`.

#### `Indexer_TwoByTwoGrid_ReturnsCoordValues` — `Vector3IndexingTests`

- **Source:** `Tests/Geometric/Vector3/Vector3IndexingTests.cs` · `[Fact]`
- **Given** Four vertices (12 floats) laid out as in the test grid comment.
- **Then** Sampled coordinates match: row0 `y=2`, row1 `z=6`, row2 `x=7`, row3 `y=11`.

#### `Indexer_ThreeByThreeGrid_ReturnsCoordValues` — `Vector3IndexingTests`

- **Source:** `Tests/Geometric/Vector3/Vector3IndexingTests.cs` · `[Fact]`
- **Given** Nine vertices with floats `1..27`.
- **Then** `vec[0,x]=1`, `vec[0,z]=3`, `vec[4,y]=14`, `vec[8,z]=27`.

#### `GetAtSetAt_RoundTrip` — `Vector3IndexingTests`

- **Source:** `Tests/Geometric/Vector3/Vector3IndexingTests.cs` · `[Fact]`
- **Given** One row `(1,2,3)` and `SetAt(0, Coord.x, 99f)`.
- **Then** `GetAt(0, Coord.x)` returns `99f`.

#### `SetAt_WithCpuScope_SyncsGpu` — `Vector3IndexingTests`

- **Source:** `Tests/Geometric/Vector3/Vector3IndexingTests.cs` · `[Fact]`
- **Given** Two rows; index `5` set to `99f` inside `CpuScopeAndSync`.
- **Then** `RetrieveReadOnlySpan()[5]` is `99f`.

### `Vector3OperatorsTests`

#### `Addition_AddsElementWise` — `Vector3OperatorsTests`

- **Source:** `Tests/Geometric/Vector3/Vector3OperatorsTests.cs` · `[Fact]`
- **Given** Two two-row operands as in the test.
- **Then** `a + b` CPU values are approximately `{2,3,4,6,7,8}`.

#### `ScalarMultiplication_ScalesAllComponents` — `Vector3OperatorsTests`

- **Source:** `Tests/Geometric/Vector3/Vector3OperatorsTests.cs` · `[Fact]`
- **Given** Row `(1,2,3)` multiplied by `2f`.
- **Then** Result is approximately `(2, 4, 6)`.

#### `Division_DividesElementWise` — `Vector3OperatorsTests`

- **Source:** `Tests/Geometric/Vector3/Vector3OperatorsTests.cs` · `[Fact]`
- **Given** Rows `(4,6,8)` and `(2,3,4)`.
- **Then** `a / b` is approximately `(2, 2, 2)`.

### `Vector3StructuralTests`

#### `Copy_CreatesEqualVector3` — `Vector3StructuralTests`

- **Source:** `Tests/Geometric/Vector3/Vector3StructuralTests.cs` · `[Fact]`
- **Given** Two-row `Vector3` `{1..6}`.
- **Then** `Copy()` CPU buffer matches the original within test tolerance.

#### `Concat_AppendsVector3Values` — `Vector3StructuralTests`

- **Source:** `Tests/Geometric/Vector3/Vector3StructuralTests.cs` · `[Fact]`
- **Given** Rows `{1,2,3}` and `{4,5,6}`.
- **Then** `Concat` has `Length` `6` and values `{1..6}` within test tolerance.

#### `AccessRow_ReturnsSingleVertexRow` — `Vector3StructuralTests`

- **Source:** `Tests/Geometric/Vector3/Vector3StructuralTests.cs` · `[Fact]`
- **Given** Two-row `{1..6}`.
- **Then** `AccessRow(1)` is approximately `(4, 5, 6)`.

## Graphs

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `GraphSampleParityTests`

#### `InterpolationLerp_MatchesEager` — `GraphSampleParityTests`

- **Source:** `Tests/Graphs/GraphSampleParityTests.cs` · `[Theory]`
- **Given** `InterpolationLerpWorkload` initialized on the default GPU with vector length `n`.
- **Then** `ComputeMaxAbsError()` vs the eager reference is less than `1e-4`.
- **Cases:**
  - n=100
  - n=256

#### `StatsLogTransform_MatchesEager` — `GraphSampleParityTests`

- **Source:** `Tests/Graphs/GraphSampleParityTests.cs` · `[Theory]`
- **Given** `StatsLogTransformWorkload` initialized with length `n`.
- **Then** Max absolute error vs eager reference is less than `1e-4`.
- **Cases:**
  - n=100
  - n=256

#### `GraphicsReinhard_MatchesEager` — `GraphSampleParityTests`

- **Source:** `Tests/Graphs/GraphSampleParityTests.cs` · `[Theory]`
- **Given** `GraphicsReinhardWorkload` initialized with length `n`.
- **Then** Max absolute error vs eager reference is less than `1e-4`.
- **Cases:**
  - n=100
  - n=256

#### `NeuralDenseLayer_MatchesEager` — `GraphSampleParityTests`

- **Source:** `Tests/Graphs/GraphSampleParityTests.cs` · `[Theory]`
- **Given** `NeuralDenseLayerWorkload` initialized with parameter `k`.
- **Then** Max absolute error vs eager reference is less than `1e-4`.
- **Cases:**
  - k=16
  - k=32

#### `SolverJacobiStep_MatchesEager` — `GraphSampleParityTests`

- **Source:** `Tests/Graphs/GraphSampleParityTests.cs` · `[Theory]`
- **Given** `SolverJacobiStepWorkload` initialized with parameter `k`.
- **Then** Max absolute error vs eager reference is less than `1e-4`.
- **Cases:**
  - k=16
  - k=32

## IO

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `CsvIoTests`

#### `Vector_1D_RoundTrip` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** A 1D `Vector` `{1, 2, 3}` on the test GPU.
- **Then** CSV serialize/deserialize preserves `Columns == 0` and values `{1,2,3}`; header line starts with `schemaVersion,type,dtype,columns,data`.

#### `Vector_2D_RoundTrip` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** A `Vector` `{1,2,3,4}` with `columns=2`.
- **Then** Round-trip preserves `Columns == 2` and all floats.

#### `Vector3_RoundTrip` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector3` data `{1..6}` (two rows).
- **Then** CSV contains `Vector3` and `1;2;3;4;5;6`; loaded `Columns == 3` and values match.

#### `Mask_Bool_RoundTrip` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** Bool mask `{true,false,true,true}` with `columns=2`.
- **Then** CSV uses `Mask,Boolean`; bits round-trip; `ElementCount` 4.

#### `Mask_Packed_RoundTrip` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** Nine-bit packed mask (pattern in test) with `columns=3`.
- **Then** CSV uses `Mask,Int32` and packed header includes `count`; bits round-trip.

#### `Vector3_InvalidLength_Throws` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** CSV declaring `Vector3` with four data tokens (not a multiple of three).
- **Then** `Deserialize` throws `FormatException` mentioning `multiple of 3`.

#### `Mask_Packed_InvalidWordCount_Throws` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** Packed mask CSV with `count=33` but only one data word.
- **Then** Throws `FormatException` mentioning `word`.

#### `Vector_InvalidFloat_Throws` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** CSV vector row containing `abc` as a float token.
- **Then** Throws `FormatException` mentioning `invalid float`.

#### `MissingHeaderRow_Throws` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** CSV missing the schema header row.
- **Then** Throws `FormatException` mentioning `header row`.

#### `Vector3_TypeMismatch_Throws` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** Serialized plain `Vector` CSV passed to `Vector3` formatter.
- **Then** Throws `FormatException` mentioning `Vector3`.

#### `Vector_NaNAndInfinity_RoundTrip` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector` with NaN, +Inf, and −Inf.
- **Then** After round-trip, index 0 is NaN, index 1 +Inf, index 2 −Inf (IEEE semantics).

#### `Vector_Empty_RoundTrips` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** Zero-length `Vector` with `columns=0`.
- **Then** Loaded `Columns` and `Length` are both `0`.

#### `Vector_EmptyDataToken_Throws` — `CsvIoTests`

- **Source:** `Tests/IO/CsvIoTests.cs` · `[Fact]` · Category=IO
- **Given** CSV data field `1;;2` (empty token).
- **Then** Throws `FormatException` mentioning `empty element`.

### `IoFileTests`

#### `Json_Vector_FileRoundTrip` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** 2×2 `Vector` `{1..4}` written under a temp directory.
- **Then** `vector.json` exists; deserialized values and `Columns` match.

#### `Json_Vector3_And_Mask_FileRoundTrip` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector3` identity rows and a 2×2 mask written as JSON (packed and bool variants).
- **Then** All three files deserialize to the original geometry and bit patterns.

#### `Csv_Vector_FileRoundTrip` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** Row vector length 5 with `columns=5`.
- **Then** File uses expected CSV preamble; loaded values match original.

#### `Csv_Vector3_And_Mask_FileRoundTrip` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector3` and mask saved as bool and packed CSV.
- **Then** On-disk CSV markers match format; deserialized data matches originals.

#### `Txt_Vector3_And_Mask_FileRoundTrip` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** Single-row `Vector3` and 1D mask as `.txt` files.
- **Then** Deserialized values match `{1,2,3}` and `{true,false,true}`.

#### `Xml_Vector3_And_Mask_FileRoundTrip` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector3` and mask XML files (packed and bool).
- **Then** Deserialized arrays and masks match the written data.

#### `Txt_Vector_WritesContent` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector` `{1,2,3}` serialized to `vector.txt`.
- **Then** File text contains substrings `1` and `2`.

#### `Serialize_WhenFileExists_AndOverwriteFalse_Throws` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** `dup.json` already written with `overwrite: false`.
- **Then** Second serialize throws `IOException` mentioning `already exists`.

#### `Serialize_WhenOverwriteTrue_ReplacesFile` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** Existing `dup.json` replaced with `overwrite: true` and new data `{9,8,7}`.
- **Then** Deserialized vector is `{9,8,7}` within test tolerance.

#### `WriteRaw_WritesPlainText` — `IoFileTests`

- **Source:** `Tests/IO/IoFileTests.cs` · `[Fact]` · Category=IO
- **Given** `WriteRaw("hello world")` on a text writer session.
- **Then** `note.txt` content is exactly `hello world`.

### `JsonIoTests`

#### `Vector_RoundTrip_PreservesColumnsAndData` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** 2×2 `Vector` written via `IO.Serialize` to a temp folder.
- **Then** JSON includes `schemaVersion`, `type` `Vector`, `dtype` `Single`; loaded `Columns` and values match.

#### `Vector_Empty_RoundTrips` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** Empty `Vector` (`length` 0, `columns` 0).
- **Then** After file round-trip, `Length` and `Columns` remain zero.

#### `Vector_NaNAndInfinity_RoundTrip` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector` with NaN and infinities via file API.
- **Then** Special float values survive round-trip (NaN / ±Inf checks).

#### `Vector3_RoundTrip_PreservesData` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector3` `{1..6}` saved as `v3.json`.
- **Then** `Columns == 3` and array values match within tolerance.

#### `Vector3_TypeMismatch_Throws` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** A `Vector` JSON file on disk.
- **Then** Deserializing as `Vector3` throws with message containing `Vector`.

#### `Mask_Packed_RoundTrip` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** Packed mask `{true,false,true,true}` with `columns=2`.
- **Then** JSON shows `count` 4 and `dtype` `Int32`; bits round-trip.

#### `Mask_Bool_RoundTrip` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** Bool mask `{false,true,false}` serialized with `MaskSerializeFlags.Bool`.
- **Then** JSON uses `dtype` `Boolean` without a `count` field; bits round-trip.

#### `Mask_Packed_InvalidWordCount_Throws` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** Raw JSON with packed mask `count=33` and a single data word.
- **Then** Deserialize throws mentioning `word`.

#### `Vector3_InvalidColumns_Throws` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** JSON `Vector3` document with `columns=2`.
- **Then** Throws `JsonException` stating columns must be `3`.

#### `Mask_Bool_RoutesOnDtype_NotCountHeuristic` — `JsonIoTests`

- **Source:** `Tests/IO/JsonIoTests.cs` · `[Fact]` · Category=IO
- **Given** JSON bool mask with misleading `count=99` but three bool data entries.
- **Then** `ElementCount` is `3` and bits are `{true,false,true}` (dtype wins over count).

### `MultiDocumentIoTests`

#### `Csv_Append_ThreeVectors_RoundTripsAll` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Three appended `Vector` documents `{1,2}`, `{3,4}`, `{5,6}` in one CSV writer session.
- **Then** `DeserializeAll` returns three items with matching values; file has five non-empty lines (schema + header + 3 rows).

#### `Json_Append_ThreeVectors_RoundTripsAll` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Same three vectors appended to a JSON collection file.
- **Then** JSON is a single `items` array wrapper; middle vector deserializes to `{3,4}`.

#### `Xml_Append_ThreeVectors_RoundTripsAll` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Three vectors appended under `<root schemaVersion="1">`.
- **Then** `DeserializeAll` count is 3; third vector is `{5,6}`; root element has no per-item `type=` attribute.

#### `Txt_Append_ThreeVectors_RoundTripsAll` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Three vectors appended to a `.txt` batch file.
- **Then** File contains `---` separators; first and third vectors round-trip correctly.

#### `Csv_Append_ThreeMasks_RoundTripsAll` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Two bool masks appended (`{true,false}` and `{false,true}`, `columns=2`).
- **Then** `DeserializeAll` returns two masks with matching bit patterns.

#### `Json_Append_ThreeMasks_RoundTripsAll` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Two packed 3-column masks appended to JSON.
- **Then** Both masks round-trip (`{T,F,T}` and `{F,F,T}` patterns).

#### `Deserialize_WhenFileHasMultipleItems_ThrowsAndPointsToDeserializeAll` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** JSON file with two appended vectors.
- **Then** Single-item `Deserialize` throws `InvalidOperationException` mentioning `DeserializeAll`.

#### `Json_SerializeAll_MatchesAppendedFile` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Three vectors appended via writer vs `SerializeAll` on the same array.
- **Then** `SerializeAll` output equals on-disk `appended.json` byte-for-byte.

#### `SerializeAll_WithEmptyCollection_Throws` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Empty `Vector` array passed to `SerializeAll`.
- **Then** Throws `ArgumentException` mentioning `at least one item`.

#### `FileSession_Serialize_DoesNotTouchDisk` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Writer created for `untouched.json` but only `Serialize` called (no flush/append).
- **Then** Fragment contains `"type":"Vector"`; target file does not exist yet.

#### `Append_WithMismatchedMaskFlags_Throws` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** First mask appended as bool, second append attempted as packed.
- **Then** Throws `InvalidOperationException` mentioning `consistent`.

#### `WriteRaw_AfterAppendStarted_Throws` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** JSON writer after one `Append`.
- **Then** `WriteRaw` throws `InvalidOperationException` mentioning `Flush`.

#### `Flush_CalledAfterDispose_IsIdempotent` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** Writer disposed after appending `{1,2}`.
- **Then** Post-dispose `Flush` does not throw; single-item deserialize has `Length` 2.

#### `Write_AfterAppendStarted_Throws` — `MultiDocumentIoTests`

- **Source:** `Tests/IO/MultiDocumentIoTests.cs` · `[Fact]` · Category=IO
- **Given** JSON writer with one appended vector.
- **Then** `Write` throws `InvalidOperationException` mentioning `Write after Append`.

### `TxtIoTests`

#### `Vector_1D_RoundTrip` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** 1D `Vector` `{1,2,3}`.
- **Then** `Columns == 0` and values round-trip.

#### `Vector_Column_RoundTrip` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** Column vector `{1,2,3}` with `columns=1`.
- **Then** `Columns == 1` and values round-trip.

#### `Vector_2D_RoundTrip` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** 2×2 `Vector` `{1..4}`.
- **Then** `Columns == 2` and values round-trip.

#### `Vector3_RoundTrip` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector3` `{1..6}`.
- **Then** `Columns == 3` and components round-trip.

#### `Mask_1D_RoundTrip` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** 1D mask `{true,false,true}`.
- **Then** `Columns == 0` and bits round-trip.

#### `Mask_2D_RoundTrip` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** 2×2 mask pattern from test.
- **Then** `Columns == 2` and bits round-trip.

#### `Vector_SpecialFloats_RoundTrip` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector` with NaN, ±Inf, and `-1.5f`.
- **Then** Special floats and `-1.5f` survive round-trip (approximate check on finite value).

#### `Vector_2D_Serialize_DoesNotStartWithNewline` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** 5×2 `Vector` serialized to text.
- **Then** Serialized string does not start with `\n` or `\r\n`.

#### `Vector3_Serialize_DoesNotStartWithNewline` — `TxtIoTests`

- **Source:** `Tests/IO/TxtIoTests.cs` · `[Fact]` · Category=IO
- **Given** Two-row `Vector3` text serialization.
- **Then** Output does not start with a newline.

### `VectorIntIoTests`

#### `Json_RoundTrip_PreservesColumnsAndData` — `VectorIntIoTests`

- **Source:** `Tests/IO/VectorIntIoTests.cs` · `[Fact]` · Category=IO
- **Given** `VectorInt` `{1,-2,3,4}` with `columns=2` via JSON file API.
- **Then** JSON shows `VectorInt` / `Int32`; loaded integers match.

#### `Csv_RoundTrip_PreservesData` — `VectorIntIoTests`

- **Source:** `Tests/IO/VectorIntIoTests.cs` · `[Fact]` · Category=IO
- **Given** `VectorInt` `{1..5}` with `columns=5`.
- **Then** CSV preamble matches schema; values equal original.

#### `Xml_RoundTrip_PreservesData` — `VectorIntIoTests`

- **Source:** `Tests/IO/VectorIntIoTests.cs` · `[Fact]` · Category=IO
- **Given** `VectorInt` `{-1,2,-3}` file round-trip.
- **Then** Loaded integers match.

#### `Txt_RoundTrip_PreservesData` — `VectorIntIoTests`

- **Source:** `Tests/IO/VectorIntIoTests.cs` · `[Fact]` · Category=IO
- **Given** `VectorInt` `{1,2,3}` text round-trip.
- **Then** Loaded integers match.

#### `Json_Collection_RoundTrips_MultipleItems` — `VectorIntIoTests`

- **Source:** `Tests/IO/VectorIntIoTests.cs` · `[Fact]` · Category=IO
- **Given** JSON writer appends `{1,2}` then `{3,4,5}`.
- **Then** `DeserializeAll` returns two vectors with those exact sequences.

### `XmlIoTests`

#### `Vector_RoundTrip_PreservesColumnsAndData` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** 2×2 `Vector` via file serialize/deserialize.
- **Then** XML uses `<vector` root, `dtype="Single"`, `schemaVersion="1"`; values and `Columns` match.

#### `Vector3_RoundTrip_PreservesData` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector3` `{1..6}` in-memory XML round-trip.
- **Then** `Columns == 3` and values match.

#### `Vector3_TypeMismatch_Throws` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** XML for a plain `Vector` passed to `Vector3` deserializer.
- **Then** Throws `FormatException` mentioning `vector3`.

#### `Mask_Packed_RoundTrip` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** Packed mask `{true,false,true,true}` (`columns=2`).
- **Then** XML includes `count="4"` and `dtype="Int32"`; bits round-trip.

#### `Mask_Bool_RoundTrip` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** Bool mask `{false,true,false}`.
- **Then** `dtype="Boolean"` without `count=` attribute; bits round-trip.

#### `Mask_Packed_InvalidWordCount_Throws` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** Packed mask XML with `count=33` and one data word.
- **Then** Deserialize throws mentioning `word`.

#### `Vector_NaNAndInfinity_RoundTrip` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** `Vector` with NaN and infinities in XML formatter round-trip.
- **Then** Special float semantics preserved on load.

#### `Mask_Bool_RoutesOnDtype_NotCountHeuristic` — `XmlIoTests`

- **Source:** `Tests/IO/XmlIoTests.cs` · `[Fact]` · Category=IO
- **Given** Bool mask XML with `count="99"` but three `<data>` bool elements.
- **Then** `ElementCount` is `3` and bits are `{true,false,true}`.

## Modules / Statistics

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `GlobalReduceTests`

#### `Sum_MatchesSumX` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Theory]` · `GpuSerial`
- **Given** A random `Vector` whose length is `length` (seed 31).
- **Then** `Sum()` and `SumX()` agree within `1e-4`.
- **Cases:**
  - length=1
  - length=7
  - length=256
  - length=4096

#### `SumX_LargeN_MatchesErrorCorrectedSum` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Length 10_000 vector filled with `0.1f`.
- **Then** `SumX()` is approximately `1000f` (±0.1); `Sum()` matches `SumX()` within `0.1`.

#### `SumX_LargeN_WithinSixDecimalPlaces` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** One million elements each `0.1f`.
- **Then** |`SumX()` − 100_000| is less than `1e-5`.

#### `SumX_10M_RepeatedTenth_MatchesCpuSum` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Ten million elements each `0.1f`.
- **Then** `SumX()` is approximately `1_000_000f` (±0.1); `Sum()` matches `SumX()` within `0.1`.

#### `SumX_SmallN_AccurateOnRepeatedTenth` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Hundred elements each `0.1f`.
- **Then** `SumX()` is approximately `10f` within `1e-4`.

#### `Sum_LargeRepeatedValue_ErrorCorrectedMoreAccurateThanNaiveLoop` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Ten thousand copies of `0.1f`.
- **Then** `Sum()` is closer to `1000f` than a naive CPU loop sum; `Sum()` is approximately `1000f` (±0.1).

#### `Dot_LargeRepeatedValue_ErrorCorrectedMoreAccurateThanNaiveLoop` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Left vector ten thousand `0.1f` values, right vector all `1f`.
- **Then** `Dot` is closer to `1000f` than naive accumulation; result approximately `1000f` (±0.1).

#### `DotX_SmallN_AccurateOnRepeatedTenth` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Length-100 dot product as above.
- **Then** `DotX` is approximately `10f` within `1e-4`.

#### `VarX_ConstantVector_IsZero` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Hundred elements all `0.1f`.
- **Then** `VarX()` is approximately `0f` within `1e-4`.

#### `AllX_LargeN_MatchesCpuAll` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random `VectorInt` and matching `Vector` of length `length` (zeros allowed).
- **Then** `All()` equals `AllX()` for both types.
- **Cases:**
  - length=10_000
  - length=1_000_000

#### `VectorInt_Sum_Overflow_WrapsUnchecked_CpuMatchesGpu` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** `VectorInt` `{int.MaxValue, 2}`.
- **Then** Both `Sum()` and `SumX()` equal unchecked `int.MaxValue + 2`.

#### `Dot_Scalar_UsesScalarTimesSum` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** `Vector` `{2,3,5}` dotted with scalar `4f`.
- **Then** `Dot(4f)` is exactly `40f`; `DotX(4f)` is approximately `40f` within `1e-5`.

#### `MinMaxMean_VarStd_All_MatchGpu` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random float vector of length `length` (seed 31, optional zeros).
- **Then** CPU reducers (`Min`, `Max`, `Mean`, `Var`, `Std`, `All`, etc.) match `*X` GPU paths within stated tolerances (`1e-5`–`1e-4`).
- **Cases:**
  - length=16
  - length=1024

#### `Percentile_MedianQuartiles_MatchSortedReference` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random data sorted on CPU for reference percentiles.
- **Then** `Median`, quartiles, `Iqr`, and `MedianX` match reference within `1e-5`.
- **Cases:**
  - length=16
  - length=128

#### `Percentile_OutOfRange_ThrowsFixedRange` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Theory]` · `GpuSerial`
- **Given** `Vector` `{3,1,2}` and invalid `percentile`.
- **Then** `Percentile` throws `FixedRangeException` with `ParamName` `percentile`.
- **Cases:**
  - percentile=-1f
  - percentile=101f
  - percentile=NaN

#### `VectorInt_Reduces_MatchGpu` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random `VectorInt` of length `length`.
- **Then** Sum/min/max/mean/var/std/all CPU APIs match `*X` counterparts within test tolerances.
- **Cases:**
  - length=16
  - length=1024

#### `Var_LargeN_DoesNotRequireArithmeticKernels` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Twelve thousand constant `0.1f` values.
- **Then** `Var()` and `VarX()` are approximately `0f` within `1e-5`.

#### `Var_LargeMeanOffset_WithinSixDecimalPlaces_CpuMatchesGpu` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Five floats near `100_000` (`100_001` … `100_005`).
- **Then** `Var()`, `VarX()`, and cross-checks are approximately `2f` within `1e-5`.

#### `Sum_Empty_ReturnsZero` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Zero-length `Vector`.
- **Then** `Sum()` and `SumX()` are `0f`.

#### `All_Empty_ReturnsTrue` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Zero-length `Vector`.
- **Then** `All()` and `AllX()` are true.

#### `Mean_Empty_ThrowsDivideByZero` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Zero-length `Vector`.
- **Then** `Mean()` and `MeanX()` throw `DivideByZeroException`.

#### `Min_Empty_FloatCpuThrows_InvalidOperation_GpuReturnsNaN` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Zero-length float `Vector`.
- **Then** CPU `Min()` throws `InvalidOperationException`; `MinX()` returns `NaN`.

#### `Min_Empty_VectorInt_ThrowsInvalidOperation` — `GlobalReduceTests`

- **Source:** `Tests/Modules/Statistics/GlobalReduceTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Zero-length `VectorInt`.
- **Then** Both `Min()` and `MinX()` throw `InvalidOperationException`.

### `RowReduceAccuracyTests`

#### `WideCols_MatchCpuErrorCorrected` — `RowReduceAccuracyTests`

- **Source:** `Tests/Modules/Statistics/RowReduceAccuracyTests.cs` · `[Theory]` · `GpuSerial`
- **Given** 64×10_000 matrix and coefficient row of `0.1f`; row reduce op `op`.
- **Then** GPU `RowReduceAlgorithms.Reduce` matches `CpuSimdReduce.RowReduceFloatCompensated` within `0.1f`.
- **Cases:**
  - op=multiply
  - op=add

#### `WideCols_AllOps_MatchCpuErrorCorrected` — `RowReduceAccuracyTests`

- **Source:** `Tests/Modules/Statistics/RowReduceAccuracyTests.cs` · `[Theory]` · `GpuSerial`
- **Given** 8×512 sequential matrix/coeff pair; row reduce op `op`.
- **Then** GPU output matches compensated CPU reference within `1e-3f`.
- **Cases:**
  - op=multiply, add, subtract, flipSubtract, divide, flipDivide, differenceSquared

#### `Distance_WideCols_MatchesCpuErrorCorrected` — `RowReduceAccuracyTests`

- **Source:** `Tests/Modules/Statistics/RowReduceAccuracyTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Same 8×512 sequential data as the all-ops theory (distance op).
- **Then** GPU distance row reduce matches CPU reference within `1e-3f`.

#### `CrossX_1Dx2D_MatchesRowReduceMultiply` — `RowReduceAccuracyTests`

- **Source:** `Tests/Modules/Statistics/RowReduceAccuracyTests.cs` · `[Fact]` · `GpuSerial`
- **Given** 1D coeff length 16 and 4×16 matrix (sequential fills from test).
- **Then** `CrossX` result matches `RowReduce` with `multiply` within `1e-4f`.

### `StatisticsMathEdgeCaseTests`

#### `MedianX_UnsortedGpuResident_MatchesCpu_KnownRegressionDataset` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Unsorted eleven-value regression dataset; buffer pinned to GPU residence.
- **Then** `MedianX()` is approximately `324f`; matches CPU `Median()` within `1e-5`.

#### `OrderStatsX_OnGpu_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Float edge-case array `data` with GPU residence; case label is the second `MemberData` string in source.
- **Then** Percentiles (0/50/100), median, quartiles, and IQR `*X` APIs match CPU within `1e-5`.
- **Cases:**
  - single element `[42]`
  - two values `[10,20]`
  - all negative values
  - all zeros
  - mixed signs with zero
  - mostly positive with zeros
  - ascending small set
  - descending unsorted set
  - regression unsorted eleven-value set

#### `VectorInt_OrderStatsX_OnGpu_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Integer edge-case array `data` on GPU.
- **Then** Median/quartile/IQR `*X` paths match CPU within `1e-5`.
- **Cases:**
  - single `[7]`
  - mixed signs `[-10..10]`
  - all zeros
  - unsorted `[9,1,8,2,7,3]`

#### `DescriptiveStatsX_OnGpu_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Float descriptive edge-case `data` on GPU.
- **Then** Sum/min/max/mean/var/std/sample stats and `All` match CPU `*X` within reduce/var tolerances (`1e-5`).
- **Cases:**
  - single value
  - negatives and positive mix
  - all zeros
  - mixed with zero
  - constant vector
  - wide spread values

#### `VectorInt_DescriptiveStatsX_OnGpu_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Integer descriptive edge-case `data` on GPU.
- **Then** Full descriptive `*X` suite matches CPU (including sample stats when length ≥ 2).
- **Cases:**
  - mixed signs `[-3..4]`
  - all zeros
  - constant fives

#### `OrderStats_WithPositiveInfinity_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Data including `+Infinity` on GPU.
- **Then** Min/max/median `*X` match CPU within tolerances.

#### `OrderStats_WithNegativeInfinity_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Data including `-Infinity` on GPU.
- **Then** Min/max/median `*X` match CPU within tolerances.

#### `OrderStats_WithNaN_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Data including NaN on GPU.
- **Then** Median and quartile `*X` match CPU within `1e-5`.

#### `DescriptiveStats_AllZeros_OnGpu_MatchCpu` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Five zeros on GPU.
- **Then** Sum/mean/var/std/sample stats are zero; `All` and `AllX` are false.

#### `PercentileX_OutOfRange_ThrowsBeforeSort` — `StatisticsMathEdgeCaseTests`

- **Source:** `Tests/Modules/Statistics/StatisticsMathEdgeCaseTests.cs` · `[Theory]` · `GpuSerial`
- **Given** GPU-resident `{3,1,2}` and invalid percentile.
- **Then** `PercentileX` throws `FixedRangeException` (`percentile` param).
- **Cases:**
  - percentile=-1f
  - percentile=101f
  - percentile=NaN

### `VarReducePathTests`

#### `DeviceDefaultPath_UsesSharedScratchOnTypicalHardware` — `VarReducePathTests`

- **Source:** `Tests/Modules/Statistics/VarReducePathTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Current test GPU accelerator.
- **Then** `SelectVarianceReducePath` returns `SharedScratch` on typical hardware.

#### `VarFloat_EachKernelPath_MatchesCpuReference` — `VarReducePathTests`

- **Source:** `Tests/Modules/Statistics/VarReducePathTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random float vector of length `length` (seed 47).
- **Then** Each harness variance path matches CPU population variance within `1e-5f`.
- **Cases:**
  - length=1
  - length=17
  - length=256
  - length=4096
  - length=65_536

#### `VarInt_EachKernelPath_MatchesCpuReference` — `VarReducePathTests`

- **Source:** `Tests/Modules/Statistics/VarReducePathTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random `VectorInt` of length `length`.
- **Then** Each harness path matches CPU population variance within `1e-5f`.
- **Cases:**
  - length=1
  - length=256
  - length=4096

#### `VarFloat_BothKernelPaths_Agree` — `VarReducePathTests`

- **Source:** `Tests/Modules/Statistics/VarReducePathTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random floats with large mean offset (`+100_000`) of length `length`.
- **Then** Shared-scratch vs warp-tree harness variances agree within `1e-3f`.
- **Cases:**
  - length=256
  - length=10_000

#### `VarX_UsesProductionKernelSelectedAtLoad` — `VarReducePathTests`

- **Source:** `Tests/Modules/Statistics/VarReducePathTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Five large-mean-offset floats (`100_001` … `100_005`).
- **Then** Production `VarX()` is approximately `2f` within `1e-5f`.

### `VarianceDefinitionTests`

#### `Var_IsPopulationVariance_SampleVar_IsSampleVariance` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** `{1,2,3,4,5}`.
- **Then** `Var()` ≈ `2f`; `SampleVar()` ≈ `2.5f`; sample std ≈ `sqrt(2.5)`; sample var scales population var by n/(n−1).

#### `VectorInt_VarAndSampleVar_MatchKnownDataset` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Integer dataset `{2,4,4,4,5,5,7,9}`.
- **Then** `Var()` ≈ `4`; `SampleVar()` ≈ `32/7`; `SampleStd()` ≈ `sqrt(32/7)` (±`1e-3`).

#### `SampleVar_Empty_ReturnsZero` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Empty `Vector`.
- **Then** `SampleVar()` and `SampleVarX()` are `0f`.

#### `SampleVar_ConstantVector_IsZero` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Eight copies of `3.5f`.
- **Then** `SampleVar()` and `SampleVarX()` are approximately `0f`.

#### `SampleVar_TwoElements_MatchesHandCalculation` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** `{10f, 20f}`.
- **Then** `SampleVar()` ≈ `50f`; `Var()` ≈ `25f`.

#### `SampleVar_SingleElement_ThrowsDivideByZero` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Single-element vector `{42f}`.
- **Then** `SampleVar()` and `SampleVarX()` throw `DivideByZeroException`.

#### `Mean_HasNoSamplePopulationSplit` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** `{1..5}`.
- **Then** `Mean()` equals arithmetic average; `MeanX()` matches within `1e-5`.

#### `SampleVar_MatchesSampleVarX` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random float vector of length `length` (seed 31).
- **Then** `SampleVar`/`SampleStd` match `*X` paths; sample var equals population var × n/(n−1).
- **Cases:**
  - length=2
  - length=16
  - length=1024

#### `VectorInt_SampleVar_MatchesSampleVarX` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Theory]` · `GpuSerial`
- **Given** Random `VectorInt` of length `length`.
- **Then** `SampleVar`/`SampleStd` match `*X` within tolerance.
- **Cases:**
  - length=2
  - length=16
  - length=256

#### `Var_LargeSpread_CpuAndGpuMayDifferWithinFloat32` — `VarianceDefinitionTests`

- **Source:** `Tests/Modules/Statistics/VarianceDefinitionTests.cs` · `[Fact]` · `GpuSerial`
- **Given** Eleven-value wide-spread regression dataset.
- **Then** |`Var()` − `VarX()`| is less than `1f`; CPU `Var()` ≈ `2_372_909.25` (±0.5); sample variance relations hold within `1f`.

## Services

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `GPUManagerDeviceSelectionTests`

#### `Context_IsAvailableAfterInitialization` — `GPUManagerDeviceSelectionTests`

- **Source:** `Tests/Services/GPUManagerDeviceSelectionTests.cs` · `[Fact]`
- **Given** Shared GPU test fixture has initialized `GPUManager`.
- **Then** `GPUManager.Context` is non-null and exposes at least one device.

#### `DefaultGpu_HasLoadedKernels` — `GPUManagerDeviceSelectionTests`

- **Source:** `Tests/Services/GPUManagerDeviceSelectionTests.cs` · `[Fact]`
- **Given** `Vector` `{1,2,3}` on the default GPU.
- **Then** Element-wise `vector + vector` yields `{2,4,6}` within test tolerance.

## Utility

### Coverage gaps (manual)

_Add untested edge cases here during review._

### `UtilTests`

#### `IsClose_ReturnsTrueWithinTolerance` — `UtilTests`

- **Source:** `Tests/Utility/UtilTests.cs` · `[Fact]`
- **Given** Pairs `(1f, 1.000001f)` and `(1f, 2f)`.
- **Then** `IsClose` is true for the near pair and false for the distant pair.

#### `MinMax_NonInf_CapsInfinityContributionAt999` — `UtilTests`

- **Source:** `Tests/Utility/UtilTests.cs` · `[Fact]`
- **Given** `{5, 2, +Inf, NaN}` with `NonInf: true`.
- **Then** `Max` returns `999f`; `Min` returns `2f`.

#### `MinMaxInf_DetectsInfinity` — `UtilTests`

- **Source:** `Tests/Utility/UtilTests.cs` · `[Fact]`
- **Given** `{1, +Inf, 3}`.
- **Then** `MinMaxInf` reports `hasInf` true; min `1`; max capped at `999`.

#### `MinMax_EmptyArray_Throws` — `UtilTests`

- **Source:** `Tests/Utility/UtilTests.cs` · `[Fact]`
- **Given** Empty `float[]`.
- **Then** `Util.Min` throws `Exception` mentioning `Length 0`.

---

---

**Last audited:** 2026-10-11 — 531 test methods across 82 test classes.
