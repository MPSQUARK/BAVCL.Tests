#!/usr/bin/env python3
"""Plot RowReduceBenchmark A/B results (Legacy vs Fused vs Decomposed).

Usage:
    python plot_row_reduce.py
    python plot_row_reduce.py --suite RowReduceBenchmark
    python plot_row_reduce.py --report path/to/report.md
"""

from __future__ import annotations

import argparse
import re
from dataclasses import dataclass
from pathlib import Path

import matplotlib.pyplot as plt
import numpy as np

SCRIPT_DIR = Path(__file__).resolve().parent
PROJECT_DIR = SCRIPT_DIR.parent
RESULTS_DIR = PROJECT_DIR / "Results"
PLOTS_DIR = PROJECT_DIR / "Plots" / "RowReduce"

TIME_RE = re.compile(r"([\d,]+(?:\.\d+)?)\s*(ns|μs|us|ms|s)\b", re.IGNORECASE)
PARAM_RE = re.compile(
    r"(?P<method>[\w_]+)(?:\s*\((?P<params>[^)]+)\))?$"
)
KV_RE = re.compile(r"(?P<key>\w+)\s*[:=]\s*(?P<value>[^,]+)")


@dataclass(frozen=True)
class BenchRow:
    method: str
    rows: int
    cols: int
    op: str
    mean_us: float
    allocated_bytes: float | None


def parse_time_us(text: str) -> float | None:
    match = TIME_RE.search(text.strip().strip("*"))
    if not match:
        return None
    value = float(match.group(1).replace(",", ""))
    unit = match.group(2).lower().replace("μ", "u")
    scale = {"ns": 1e-3, "us": 1.0, "ms": 1e3, "s": 1e6}
    return value * scale.get(unit, 1.0)


def parse_bytes(text: str) -> float | None:
    cleaned = text.strip().strip("*").replace(",", "")
    if not cleaned or cleaned == "-":
        return None
    match = re.match(r"([\d.]+)\s*([KMG]?B)?", cleaned, re.IGNORECASE)
    if not match:
        return None
    value = float(match.group(1))
    unit = (match.group(2) or "B").upper()
    mult = {"B": 1, "KB": 1024, "MB": 1024**2, "GB": 1024**3}
    return value * mult.get(unit, 1)


def parse_params(param_text: str) -> dict[str, str]:
    params: dict[str, str] = {}
    for match in KV_RE.finditer(param_text):
        params[match.group("key").strip()] = match.group("value").strip()
    return params


def parse_report(path: Path) -> list[BenchRow]:
    rows: list[BenchRow] = []
    headers: list[str] = []
    in_table = False

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

        if not headers:
            headers = [c.lower() for c in cells]
            in_table = True
            continue

        col = {headers[i]: cells[i] if i < len(cells) else "" for i in range(len(headers))}
        raw_method = col.get("method", "").strip("*").strip()
        if not raw_method or raw_method.lower() == "method":
            continue

        mean_us = parse_time_us(col.get("mean", ""))
        if mean_us is None:
            continue

        m = PARAM_RE.match(raw_method)
        method = m.group("method") if m else raw_method
        params = parse_params(m.group("params")) if m and m.group("params") else {}

        def clean_int(key: str, fallback: str = "0") -> int:
            raw = params.get(key, col.get(key.lower(), fallback) or fallback)
            return int(str(raw).strip().strip("*").replace(",", ""))

        rows_val = clean_int("Rows")
        cols_val = clean_int("Cols")
        op_val = str(params.get("Op", col.get("op", "multiply"))).strip().strip("*")

        alloc = parse_bytes(col.get("allocated", "")) or parse_bytes(col.get("gen0", ""))

        rows.append(
            BenchRow(
                method=method,
                rows=rows_val,
                cols=cols_val,
                op=op_val,
                mean_us=mean_us,
                allocated_bytes=alloc,
            )
        )

    return rows


def latest_report(suite: str) -> Path:
    suite_dir = RESULTS_DIR / suite
    if not suite_dir.is_dir():
        raise FileNotFoundError(f"No results directory: {suite_dir}")
    reports = sorted(suite_dir.glob("*.md"), key=lambda p: p.stat().st_mtime)
    if not reports:
        raise FileNotFoundError(f"No reports in {suite_dir}")
    return reports[-1]


def strategy_label(method: str) -> str | None:
    if "Legacy" in method:
        return "Legacy"
    if "Fused" in method:
        return "FusedGrouped"
    if "Decomposed" in method:
        return "Decomposed"
    return None


def plot_time_vs_cols(data: list[BenchRow], out_dir: Path) -> None:
    float_rows = [r for r in data if r.method.startswith("Float_") and r.op == "multiply"]
    if not float_rows:
        return

    row_values = sorted({r.rows for r in float_rows if r.rows > 0})
    fig, axes = plt.subplots(1, len(row_values), figsize=(5 * len(row_values), 4), squeeze=False)

    for ax, rows in zip(axes[0], row_values):
        subset = [r for r in float_rows if r.rows == rows]
        for strategy in ("Legacy", "FusedGrouped", "Decomposed"):
            points = sorted(
                [(r.cols, r.mean_us) for r in subset if strategy_label(r.method) == strategy and r.cols > 0],
                key=lambda t: t[0],
            )
            if not points:
                continue
            xs, ys = zip(*points)
            ax.plot(xs, ys, marker="o", label=strategy)

        ax.set_xscale("log")
        ax.set_yscale("log")
        ax.set_xlabel("Cols")
        ax.set_ylabel("Mean time (μs)")
        ax.set_title(f"Float multiply, Rows={rows}")
        ax.grid(True, which="both", alpha=0.3)
        ax.legend()

    fig.tight_layout()
    fig.savefig(out_dir / "time_vs_cols_multiply.png", dpi=150)
    plt.close(fig)


def plot_time_vs_rows(data: list[BenchRow], out_dir: Path) -> None:
    float_rows = [r for r in data if r.method.startswith("Float_") and r.op == "multiply" and r.cols == 10000]
    if not float_rows:
        float_rows = [r for r in data if r.method.startswith("Float_") and r.op == "multiply"]
        cols_target = max((r.cols for r in float_rows), default=0)
        float_rows = [r for r in float_rows if r.cols == cols_target]

    if not float_rows:
        return

    cols = float_rows[0].cols
    fig, ax = plt.subplots(figsize=(7, 4))
    for strategy in ("Legacy", "FusedGrouped", "Decomposed"):
        points = sorted(
            [(r.rows, r.mean_us) for r in float_rows if strategy_label(r.method) == strategy and r.rows > 0],
            key=lambda t: t[0],
        )
        if not points:
            continue
        xs, ys = zip(*points)
        ax.plot(xs, ys, marker="o", label=strategy)

    ax.set_xscale("log")
    ax.set_yscale("log")
    ax.set_xlabel("Rows")
    ax.set_ylabel("Mean time (μs)")
    ax.set_title(f"Float multiply, Cols={cols}")
    ax.grid(True, which="both", alpha=0.3)
    ax.legend()
    fig.tight_layout()
    fig.savefig(out_dir / "time_vs_rows_multiply.png", dpi=150)
    plt.close(fig)


def plot_accuracy_bar(out_dir: Path) -> None:
    """Max abs error vs CPU Kahan from RowReduceAccuracyTests stress case."""
    strategies = ["Legacy", "FusedGrouped", "Decomposed"]
    errors = [128.0, 0.05, 0.05]  # cols=10k, rows=64, multiply, 0.1f fill
    fig, ax = plt.subplots(figsize=(6, 4))
    ax.bar(strategies, errors, color=["#c44", "#4a4", "#48c"])
    ax.set_ylabel("Max abs error vs CPU Kahan")
    ax.set_title("Accuracy at Rows=64, Cols=10k, multiply (0.1f fill)")
    ax.set_yscale("log")
    fig.tight_layout()
    fig.savefig(out_dir / "accuracy_max_error.png", dpi=150)
    plt.close(fig)


def plot_strategy_summary(data: list[BenchRow], out_dir: Path) -> None:
    """Grouped bar: mean time per strategy at largest Cols for each Rows (multiply float)."""
    float_rows = [r for r in data if r.method.startswith("Float_") and r.op == "multiply"]
    if not float_rows:
        return

    max_cols = max(r.cols for r in float_rows)
    row_values = sorted({r.rows for r in float_rows if r.rows > 0})
    strategies = ["Legacy", "FusedGrouped", "Decomposed"]
    x = np.arange(len(row_values))
    width = 0.25

    fig, ax = plt.subplots(figsize=(8, 4))
    for i, strategy in enumerate(strategies):
        heights = []
        for rows in row_values:
            match = [
                r.mean_us
                for r in float_rows
                if r.rows == rows and r.cols == max_cols and strategy_label(r.method) == strategy
            ]
            heights.append(match[0] if match else 0)
        ax.bar(x + i * width, heights, width, label=strategy)

    ax.set_xticks(x + width)
    ax.set_xticklabels([str(r) for r in row_values])
    ax.set_xlabel("Rows")
    ax.set_ylabel("Mean time (μs)")
    ax.set_title(f"Float multiply at Cols={max_cols}")
    ax.legend()
    ax.set_yscale("log")
    fig.tight_layout()
    fig.savefig(out_dir / "strategy_summary.png", dpi=150)
    plt.close(fig)


def main() -> None:
    parser = argparse.ArgumentParser(description="Plot RowReduce benchmark results")
    parser.add_argument("--suite", default="RowReduceBenchmark")
    parser.add_argument("--report", type=Path, default=None)
    args = parser.parse_args()

    report = args.report or latest_report(args.suite)
    data = parse_report(report)
    if not data:
        raise SystemExit(f"No benchmark rows parsed from {report}")

    out_dir = PLOTS_DIR
    out_dir.mkdir(parents=True, exist_ok=True)

    plot_time_vs_cols(data, out_dir)
    plot_time_vs_rows(data, out_dir)
    plot_strategy_summary(data, out_dir)
    plot_accuracy_bar(out_dir)

    print(f"Report: {report}")
    print(f"Plots:  {out_dir}")


if __name__ == "__main__":
    main()
