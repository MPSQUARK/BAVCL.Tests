#!/usr/bin/env python3
"""Plot BAVCL benchmark results across multiple dated runs.

Reads markdown reports from Results/<suite>/<timestamp>.md,
parses BenchmarkDotNet GitHub-flavoured tables, and produces line charts:
  - X axis: N (log scale)
  - Y axis: mean time (linear, auto-labelled ns / μs / ms / s)
  - Faint dashed guides at unit boundaries (1 ns, 1 μs, 1 ms, 1 s)
  - One line per historical run; optional CPU vs GPU subplot charts

Usage:
    python plot_benchmarks.py --filter Sort --suite SortBenchmarks
    python plot_benchmarks.py --method IntSort_Gpu_Asc_1D
    python plot_benchmarks.py --filter Sort --cpu-gpu
"""

from __future__ import annotations

import argparse
import re
from dataclasses import dataclass
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np
from matplotlib.ticker import AutoLocator, FuncFormatter, LogLocator

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_DIR = SCRIPT_DIR.parent
RESULTS_DIR = PROJECT_DIR / "Results"
PLOTS_DIR = PROJECT_DIR / "Plots"

TIME_RE = re.compile(
    r"([\d,]+(?:\.\d+)?)\s*(ns|μs|us|ms|s)\b", re.IGNORECASE
)
TIMESTAMP_RE = re.compile(r"^\d{4}-\d{2}-\d{2}_\d{6}$")
DATED_REPORT_RE = re.compile(r"^(\d{4}-\d{2}-\d{2}_\d{6})(?:[-_](.+))?$")
LEGACY_FLAT_RE = re.compile(r"^(\d{4}-\d{2}-\d{2}_\d{6})[-_](.+)\.md$")
METHOD_N_SUFFIX_RE = re.compile(r"_(\d+)([KkMm])$")
STANDARD_N = [100, 10_000, 1_000_000]
SUITE_HEADER_RE = re.compile(r"^\*\*Suite:\*\*\s*(.+?)\s*$", re.MULTILINE)
CPU_GPU_METHOD_RE = re.compile(
    r"^(\w+Sort)_(Cpu|Gpu)_(Asc|Desc)_(\dD)$", re.IGNORECASE
)

# Internal values are microseconds; labels mark human unit boundaries.
MAGNITUDE_GUIDES = (
    (1e-3, "1 ns"),
    (1.0, "1 μs"),
    (1e3, "1 ms"),
    (1e6, "1 s"),
)


@dataclass(frozen=True)
class ReportMeta:
    path: Path
    suite: str
    timestamp: str
    tag: str | None = None

    @property
    def run_label(self) -> str:
        ts = self.timestamp.replace("_", " ", 1)
        date, time_part = ts.split(" ", 1)
        hh, mm, ss = time_part[0:2], time_part[2:4], time_part[4:6]
        base = f"{date} {hh}:{mm}:{ss}"
        if self.tag:
            base = f"{base} {self.tag}"
        return f"{base} ({self.suite})"


@dataclass(frozen=True)
class Row:
    method: str
    n: int | None
    mean_us: float
    stddev_us: float


RunsData = dict[str, dict[str, tuple[list[int], list[float], list[float]]]]


def parse_time_to_microseconds(text: str) -> float | None:
    cleaned = text.strip().strip("*").replace(",", "")
    match = TIME_RE.search(cleaned)
    if not match:
        return None
    value = float(match.group(1))
    unit = match.group(2).lower().replace("μ", "u")
    scale = {"ns": 1e-3, "us": 1.0, "ms": 1e3, "s": 1e6}
    return value * scale.get(unit, 1.0)


def parse_int(text: str) -> int | None:
    cleaned = text.strip().strip("*").replace(",", "")
    if not cleaned or cleaned == "-":
        return None
    try:
        return int(cleaned)
    except ValueError:
        return None


def n_from_method_suffix(method: str) -> int | None:
    """Parse N when encoded as a trailing _1K / _10K / _100K / _1M suffix."""
    match = METHOD_N_SUFFIX_RE.search(method)
    if not match:
        return None
    num = int(match.group(1))
    suffix = match.group(2).upper()
    multiplier = {"K": 1_000, "M": 1_000_000}
    return num * multiplier.get(suffix, 1)


def canonical_method_and_n(method: str, n_column: int | None) -> tuple[str, int | None]:
    """One plot series per logical method; N lives on the x-axis, not in the name."""
    if n_column is not None:
        return method, n_column
    n_suffix = n_from_method_suffix(method)
    if n_suffix is None:
        return method, None
    base = method[: METHOD_N_SUFFIX_RE.search(method).start()]  # type: ignore[union-attr]
    return base, n_suffix


def parse_report_meta(path: Path, results_dir: Path) -> ReportMeta | None:
    if path.parent.name.lower() in ("plots", "archive", "incoming"):
        return None
    try:
        path.relative_to(PROJECT_DIR / "Plots")
        return None
    except ValueError:
        pass

    suite = path.parent.name if path.parent != results_dir else "Unknown"
    stem = path.stem

    if path.parent == results_dir:
        legacy = LEGACY_FLAT_RE.match(path.name)
        if legacy:
            return ReportMeta(path=path, suite=legacy.group(2), timestamp=legacy.group(1))

    dated = DATED_REPORT_RE.match(stem)
    if dated:
        text = path.read_text(encoding="utf-8")
        header_match = SUITE_HEADER_RE.search(text)
        if header_match:
            suite = header_match.group(1).strip()
        tag = dated.group(2)
        return ReportMeta(path=path, suite=suite, timestamp=dated.group(1), tag=tag)

    if TIMESTAMP_RE.match(stem):
        text = path.read_text(encoding="utf-8")
        header_match = SUITE_HEADER_RE.search(text)
        if header_match:
            suite = header_match.group(1).strip()
        return ReportMeta(path=path, suite=suite, timestamp=stem)

    return None


def parse_report(path: Path) -> list[Row]:
    rows: list[Row] = []
    in_table = False
    headers: list[str] = []

    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.startswith("|"):
            in_table = False
            continue

        cells = [c.strip() for c in line.split("|")[1:-1]]
        if not cells:
            continue

        if all(set(c) <= {"-", ":", " "} for c in cells):
            in_table = True
            continue

        if not in_table and headers == []:
            headers = [c.lower() for c in cells]
            in_table = True
            continue

        if not headers:
            continue

        col = {headers[i]: cells[i] if i < len(cells) else "" for i in range(len(headers))}

        method = col.get("method", "").strip("*").strip()
        if not method or method.lower() == "method":
            continue

        mean_us = parse_time_to_microseconds(col.get("mean", ""))
        if mean_us is None:
            continue

        stddev_us = parse_time_to_microseconds(col.get("stddev", "")) or 0.0
        n_column = parse_int(col.get("n", ""))
        method, n = canonical_method_and_n(method, n_column)

        rows.append(Row(method=method, n=n, mean_us=mean_us, stddev_us=stddev_us))

    return rows


def discover_reports(directory: Path, suite_filter: str | None) -> list[ReportMeta]:
    reports: list[ReportMeta] = []

    for path in sorted(directory.rglob("*.md")):
        meta = parse_report_meta(path, directory)
        if meta is None:
            continue
        if suite_filter and suite_filter not in meta.suite:
            continue
        reports.append(meta)

    return reports


def load_all_reports(
    directory: Path,
    suite_filter: str | None,
) -> tuple[RunsData, dict[str, str]]:
    data: RunsData = {}
    run_suites: dict[str, str] = {}

    for meta in discover_reports(directory, suite_filter):
        label = meta.run_label
        run_suites[label] = meta.suite
        run_rows: dict[str, list[Row]] = {}

        for row in parse_report(meta.path):
            if row.n is None:
                continue
            run_rows.setdefault(row.method, []).append(row)

        method_data: dict[str, tuple[list[int], list[float], list[float]]] = {}
        for method, method_rows in run_rows.items():
            by_n: dict[int, Row] = {}
            for row in method_rows:
                if row.n is not None:
                    by_n[row.n] = row  # latest wins if duplicate N
            if not by_n:
                continue
            ordered = sorted(by_n.values(), key=lambda r: r.n)  # type: ignore[arg-type]
            method_data[method] = (
                [r.n for r in ordered],  # type: ignore[misc]
                [r.mean_us for r in ordered],
                [r.stddev_us for r in ordered],
            )

        if method_data:
            data[label] = method_data

    return data, run_suites


def format_axis_time_us(value: float, _pos: float) -> str:
    """Tick label: pick ns / μs / ms / s from microsecond value."""
    if value <= 0:
        return ""
    if value >= 1e6:
        return f"{value / 1e6:g} s"
    if value >= 1e3:
        return f"{value / 1e3:g} ms"
    if value >= 1:
        return f"{value:g} μs"
    if value >= 1e-3:
        return f"{value * 1e3:g} ns"
    return f"{value * 1e6:g} ps"


# Switch to log Y when the slowest point is this many times the fastest.
LOG_Y_RATIO_THRESHOLD = 8.0


def data_extent(
    values: list[float], low_bounds: list[float] | None = None
) -> tuple[float, float] | None:
    candidates = [v for v in values if v > 0]
    if low_bounds:
        candidates.extend(v for v in low_bounds if v > 0)
    if not candidates:
        return None
    return min(candidates), max(candidates)


def should_use_log_y(lo: float, hi: float) -> bool:
    if lo <= 0:
        return True
    return (hi / lo) >= LOG_Y_RATIO_THRESHOLD


def configure_log_time_axis(ax, values: list[float], low_bounds: list[float] | None = None) -> None:
    extent = data_extent(values, low_bounds)
    if extent is None:
        return
    lo, hi = extent
    lo = max(lo, 1e-4)
    ax.set_yscale("log")
    decade_lo = 10 ** np.floor(np.log10(lo))
    decade_hi = 10 ** np.ceil(np.log10(hi))
    ax.set_ylim(decade_lo * 0.82, decade_hi * 1.18)
    ax.yaxis.set_major_locator(LogLocator(base=10, subs=(1.0, 2.0, 5.0), numticks=20))
    ax.yaxis.set_minor_locator(LogLocator(base=10, subs=np.arange(2, 10), numticks=20))
    ax.yaxis.set_major_formatter(FuncFormatter(format_axis_time_us))
    ax.tick_params(axis="y", which="major", labelsize=9)
    ax.tick_params(axis="y", which="minor", length=3)


def configure_linear_time_axis(ax, values: list[float], low_bounds: list[float] | None = None) -> None:
    """Linear Y when all points sit in a narrow band."""
    extent = data_extent(values, low_bounds)
    if extent is None:
        return

    lo, hi = extent
    span = max(hi - lo, hi * 0.08, 1.0)
    y_min = max(0.0, lo - span * 0.28)
    y_max = hi + span * 0.15

    if lo > 0:
        y_min = max(lo * 0.72, y_min)

    ax.set_ylim(y_min, y_max)
    ax.yaxis.set_major_locator(AutoLocator())
    ax.yaxis.set_major_formatter(FuncFormatter(format_axis_time_us))
    ax.tick_params(axis="y", which="major", labelsize=9)


def configure_time_axis(ax, values: list[float], low_bounds: list[float] | None = None) -> None:
    extent = data_extent(values, low_bounds)
    if extent is None:
        return
    lo, hi = extent
    if should_use_log_y(lo, hi):
        configure_log_time_axis(ax, values, low_bounds)
    else:
        configure_linear_time_axis(ax, values, low_bounds)


def configure_log_x_axis(ax, n_values: list[int] | None = None) -> None:
    ax.set_xscale("log")
    ticks = sorted(set(n_values)) if n_values else STANDARD_N
    lo, hi = ticks[0], ticks[-1]
    ax.set_xticks(ticks)
    ax.set_xticklabels([f"{n:,}" for n in ticks])
    ax.set_xlim(lo * 0.55, hi * 2.0)
    ax.set_xlabel("N (elements)")
    ax.margins(x=0.04)


def add_magnitude_guides(ax) -> None:
    """Faint horizontal dashes at ns / μs / ms / s boundaries."""
    y_min, y_max = ax.get_ylim()
    if y_min <= 0:
        y_min = max(y_min, 1e-4)
    is_log = ax.get_yscale() == "log"

    for value, label in MAGNITUDE_GUIDES:
        if value <= 0:
            continue
        if is_log:
            if value < y_min * 0.92 or value > y_max * 1.08:
                continue
        elif value < y_min or value > y_max:
            continue
        ax.axhline(
            value,
            color="#c0c0c0",
            linestyle=(0, (4, 6)),
            linewidth=0.9,
            alpha=0.55,
            zorder=0,
        )
        ax.text(
            1.005,
            value,
            label,
            transform=ax.get_yaxis_transform(),
            va="center",
            ha="left",
            fontsize=7,
            color="#999999",
            alpha=0.85,
            clip_on=False,
        )


def short_run_name(label: str) -> str:
    lower = label.lower()
    if "first-gpu-baseline" in lower or "pre-refactor" in lower:
        return "pre-refactor"
    if "post-refactor" in lower:
        return "post-refactor"
    if "smoke" in lower:
        return "smoke test"
    if " (" in label:
        head = label.split(" (", 1)[0]
        if " " in head:
            return head.split(" ", 1)[1]
    return label[:24]


def latest_run_label(runs: RunsData) -> str:
    return sorted(runs.keys())[-1]


def method_suite(method: str, runs: RunsData, run_suites: dict[str, str]) -> str:
    for label in sorted(runs.keys()):
        if method in runs[label]:
            return run_suites[label]
    return "Unknown"


def resolve_overview_suite(
    args_suite: str | None, run_suites: dict[str, str], title_suffix: str
) -> str:
    if args_suite:
        return args_suite
    suites = sorted(set(run_suites.values()))
    if len(suites) == 1:
        return suites[0]
    return re.sub(r"[^\w.-]+", "_", title_suffix)


def series_value_bounds(
    means: list[float], stds: list[float]
) -> tuple[list[float], list[float]]:
    highs = [m + s for m, s in zip(means, stds, strict=False)]
    lows = [max(0.0, m - s) for m, s in zip(means, stds, strict=False)]
    return means + highs, lows


def plot_series(
    ax,
    runs: RunsData,
    method: str,
    labels: list[str],
    cmap_name: str = "tab10",
    line_styles: dict[str, str] | None = None,
) -> tuple[list[float], list[float]]:
    all_values: list[float] = []
    all_lows: list[float] = []
    cmap = plt.colormaps.get_cmap(cmap_name)

    for i, label in enumerate(labels):
        if method not in runs[label]:
            continue
        xs, means, stds = runs[label][method]
        values, lows = series_value_bounds(means, stds)
        all_values.extend(values)
        all_lows.extend(lows)
        linestyle = "-" if line_styles is None else line_styles.get(label, "-")
        ax.errorbar(
            xs,
            means,
            yerr=stds,
            label=short_run_name(label) if line_styles is None else label,
            marker="o",
            capsize=4,
            linewidth=2,
            linestyle=linestyle,
            color=cmap(i % 10),
        )
    return all_values, all_lows


def finalize_time_axis(ax, all_values: list[float], all_lows: list[float] | None = None) -> None:
    configure_time_axis(ax, all_values, all_lows)
    add_magnitude_guides(ax)
    extent = data_extent(all_values, all_lows)
    y_label = "Mean time"
    if extent and should_use_log_y(extent[0], extent[1]):
        y_label = "Mean time (log scale)"
    ax.set_ylabel(y_label)
    ax.grid(True, axis="y", alpha=0.35)
    ax.grid(True, axis="x", alpha=0.2)


def plot_method(
    method: str,
    runs: RunsData,
    output_dir: Path,
) -> None:
    fig, ax = plt.subplots(figsize=(10.5, 5.5))

    sorted_labels = sorted(runs.keys())
    all_values, all_lows = plot_series(ax, runs, method, sorted_labels)
    all_n: list[int] = []
    for label in sorted_labels:
        if method in runs[label]:
            all_n.extend(runs[label][method][0])

    configure_log_x_axis(ax, all_n)
    finalize_time_axis(ax, all_values, all_lows)
    ax.set_title(f"Benchmark method: {method}", fontsize=12, fontweight="bold")
    ax.legend(title="Run", loc="upper left", fontsize=8)

    fig.tight_layout()
    safe_name = re.sub(r"[^\w.-]+", "_", method)
    output_dir.mkdir(parents=True, exist_ok=True)
    fig.savefig(output_dir / f"{safe_name}.png", dpi=150, bbox_inches="tight")
    plt.close(fig)


def discover_cpu_gpu_groups(methods: set[str]) -> dict[str, dict[str, tuple[str, str]]]:
    """family_order -> dim -> (cpu_method, gpu_method)."""
    partial: dict[str, dict[str, dict[str, str]]] = {}

    for method in methods:
        match = CPU_GPU_METHOD_RE.match(method)
        if not match:
            continue
        family, backend, order, dim = match.groups()
        key = f"{family}_{order}"
        partial.setdefault(key, {}).setdefault(dim, {})[backend.lower()] = method

    groups: dict[str, dict[str, tuple[str, str]]] = {}
    for key, dims in sorted(partial.items()):
        mapped: dict[str, tuple[str, str]] = {}
        for dim, backends in dims.items():
            cpu = backends.get("cpu")
            gpu = backends.get("gpu")
            if cpu and gpu:
                mapped[dim] = (cpu, gpu)
        if mapped:
            groups[key] = mapped
    return groups


def plot_cpu_gpu_group(
    group_key: str,
    dim_methods: dict[str, tuple[str, str]],
    runs: RunsData,
    output_dir: Path,
    run_label: str | None = None,
) -> None:
    run = run_label or latest_run_label(runs)
    dims = [d for d in ("1D", "2D") if d in dim_methods]
    if not dims:
        return

    fig, axes = plt.subplots(1, len(dims), figsize=(5.5 * len(dims), 5.5), squeeze=False)
    title = group_key.replace("_", " ")

    for ax, dim in zip(axes[0], dims, strict=True):
        cpu_method, gpu_method = dim_methods[dim]
        all_values: list[float] = []
        all_lows: list[float] = []
        all_n: list[int] = []
        cpu_color, gpu_color = "#1f77b4", "#ff7f0e"

        if run in runs and cpu_method in runs[run]:
            xs, means, stds = runs[run][cpu_method]
            all_n.extend(xs)
            values, lows = series_value_bounds(means, stds)
            all_values.extend(values)
            all_lows.extend(lows)
            ax.errorbar(
                xs, means, yerr=stds, label="CPU", marker="o", capsize=4,
                linewidth=2, color=cpu_color,
            )
        if run in runs and gpu_method in runs[run]:
            xs, means, stds = runs[run][gpu_method]
            all_n.extend(xs)
            values, lows = series_value_bounds(means, stds)
            all_values.extend(values)
            all_lows.extend(lows)
            ax.errorbar(
                xs, means, yerr=stds, label="GPU", marker="s", capsize=4,
                linewidth=2, linestyle="--", color=gpu_color,
            )

        configure_log_x_axis(ax, all_n)
        finalize_time_axis(ax, all_values, all_lows)
        ax.set_title(f"{dim} — {short_run_name(run)}", fontsize=11)
        ax.legend(loc="upper left", fontsize=9)

    fig.suptitle(f"CPU vs GPU: {title}", fontsize=13, fontweight="bold", y=1.02)
    fig.tight_layout()

    safe_name = re.sub(r"[^\w.-]+", "_", group_key)
    output_dir.mkdir(parents=True, exist_ok=True)
    fig.savefig(output_dir / f"cpu_gpu_{safe_name}.png", dpi=150, bbox_inches="tight")
    plt.close(fig)


def plot_overview(
    methods: list[str],
    runs: RunsData,
    output_dir: Path,
    title_suffix: str,
    n_focus: int = 10_000,
) -> None:
    labels = sorted(runs.keys())
    if not labels:
        return

    fig, ax = plt.subplots(figsize=(max(10, len(methods) * 0.35), 6))
    x = np.arange(len(methods))
    width = 0.8 / max(len(labels), 1)
    all_values: list[float] = []

    for i, label in enumerate(labels):
        values = []
        for method in methods:
            if method in runs[label]:
                xs, means, _ = runs[label][method]
                if n_focus in xs:
                    v = means[xs.index(n_focus)]
                    values.append(v)
                    if v > 0:
                        all_values.append(v)
                elif means:
                    v = means[-1]
                    values.append(v)
                    if v > 0:
                        all_values.append(v)
                else:
                    values.append(float("nan"))
            else:
                values.append(float("nan"))
        offset = (i - len(labels) / 2 + 0.5) * width
        ax.bar(x + offset, values, width, label=short_run_name(label), alpha=0.85)

    finalize_time_axis(ax, all_values)
    ax.set_ylabel(f"Mean time at N={n_focus:,}")
    ax.set_title(f"Benchmark overview — {title_suffix}", fontsize=12, fontweight="bold")
    ax.set_xticks(x)
    ax.set_xticklabels(methods, rotation=60, ha="right", fontsize=7)
    ax.legend(title="Run", fontsize=7)
    fig.tight_layout()
    output_dir.mkdir(parents=True, exist_ok=True)
    safe_suffix = re.sub(r"[^\w.-]+", "_", title_suffix)
    fig.savefig(output_dir / f"overview_{safe_suffix}_N{n_focus}.png", dpi=150, bbox_inches="tight")
    plt.close(fig)


def main() -> None:
    parser = argparse.ArgumentParser(description="Plot BAVCL benchmark markdown reports.")
    parser.add_argument("--dir", type=Path, default=RESULTS_DIR, help="Results root")
    parser.add_argument(
        "--output",
        type=Path,
        default=PLOTS_DIR,
        help="Plots root (PNG files go in <output>/<suite>/)",
    )
    parser.add_argument("--method", type=str, help="Plot a single benchmark method")
    parser.add_argument("--filter", type=str, help="Methods whose name contains this substring")
    parser.add_argument("--suite", type=str, help="Reports from suites matching this substring")
    parser.add_argument("--overview", action="store_true", help="Overview bar chart at N=10_000")
    parser.add_argument(
        "--cpu-gpu",
        action="store_true",
        help="Also write CPU vs GPU subplot charts (latest run)",
    )
    parser.add_argument(
        "--run",
        type=str,
        help="Run label substring for CPU vs GPU charts (default: latest)",
    )
    args = parser.parse_args()

    runs, run_suites = load_all_reports(args.dir, args.suite)
    if not runs:
        reports = discover_reports(args.dir, args.suite)
        if reports:
            print(
                f"Found {len(reports)} report(s) in {args.dir} but no plottable benchmark rows."
            )
        else:
            print(f"No benchmark reports found in {args.dir}")
        return

    all_methods: set[str] = set()
    for run_data in runs.values():
        all_methods.update(run_data.keys())

    if args.method:
        methods = [args.method] if args.method in all_methods else []
        if not methods:
            print(f"Method '{args.method}' not found. Available: {sorted(all_methods)}")
            return
    elif args.filter:
        methods = sorted(m for m in all_methods if args.filter in m)
        if not methods:
            suite_names = sorted(set(run_suites.values()))
            sample = sorted(all_methods)[:12]
            print(
                f"No methods match filter {args.filter!r} "
                f"(suite={args.suite or suite_names}, {len(all_methods)} method(s) in run)."
            )
            if sample:
                print(f"Examples: {', '.join(sample)}")
            if args.filter and "Sort" in args.filter and args.suite and "Sort" not in args.suite:
                print(
                    f"Hint: {args.suite} has no Sort methods - "
                    "drop --filter Sort or use --suite SortBenchmarks."
                )
            return
    else:
        methods = sorted(all_methods)

    title_suffix = args.suite or args.filter or "all methods"
    plots_root = args.output
    print(
        f"Plotting {len(methods)} method(s) from {len(runs)} run(s) "
        f"-> {plots_root}/<suite>/"
    )
    for method in methods:
        suite = method_suite(method, runs, run_suites)
        plot_method(method, runs, plots_root / suite)

    want_cpu_gpu = args.cpu_gpu
    if not want_cpu_gpu and args.filter and "Sort" in args.filter and methods:
        want_cpu_gpu = any(CPU_GPU_METHOD_RE.match(m) for m in methods)
    if want_cpu_gpu:
        cpu_gpu_methods = set(methods) if methods else all_methods
        groups = discover_cpu_gpu_groups(cpu_gpu_methods)
        if not groups:
            print("CPU vs GPU: no matching Sort Cpu/Gpu method pairs in selection.")
        else:
            run_label = latest_run_label(runs)
            if args.run:
                matches = [label for label in runs if args.run in label]
                if matches:
                    run_label = matches[-1]
            first_pair = next(iter(next(iter(groups.values())).values()))
            cpu_gpu_suite = args.suite if args.suite else method_suite(
                first_pair[0], runs, run_suites
            )
            cpu_gpu_dir = plots_root / cpu_gpu_suite
            print(
                f"CPU vs GPU charts ({len(groups)} groups, run={short_run_name(run_label)}) "
                f"-> {cpu_gpu_dir}/"
            )
            for group_key, dim_methods in groups.items():
                plot_cpu_gpu_group(group_key, dim_methods, runs, cpu_gpu_dir, run_label)

    if args.overview or (not args.method and not args.filter):
        overview_suite = resolve_overview_suite(args.suite, run_suites, title_suffix)
        plot_overview(
            sorted(all_methods),
            runs,
            plots_root / overview_suite,
            title_suffix,
        )

    print("Done.")


if __name__ == "__main__":
    main()
