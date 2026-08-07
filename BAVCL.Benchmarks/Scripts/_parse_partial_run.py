"""One-off: parse a raw BenchmarkDotNet console log (crashed before export) into a
GitHub-flavoured markdown table matching the standard report format.

Usage: python _parse_partial_run.py <log_path> <out_md_path>
"""
import re
import sys

NUM = r"[\d,.]+"
BENCH_RE = re.compile(r"^// Benchmark: SortBenchmarks\.(\w+): .*\[N=(\d+)\]$")
MEAN_RE = re.compile(
    rf"^Mean = ({NUM}) (us|ms|ns), StdErr = ({NUM}) (us|ms|ns) \({NUM}%\), N = \d+, StdDev = ({NUM}) (us|ms|ns)$"
)
MINMAX_RE = re.compile(
    rf"^Min = ({NUM}) (us|ms|ns), Q1 = {NUM} \w+, Median = {NUM} \w+, Q3 = {NUM} \w+, Max = ({NUM}) (us|ms|ns)$"
)
# groups: 1=min value, 2=min unit, 3=max value, 4=max unit

UNIT_TO_US = {"ns": 0.001, "us": 1.0, "ms": 1000.0}


def to_us(value: str, unit: str) -> float:
    return float(value.replace(",", "")) * UNIT_TO_US[unit]


def read_lines(log_path: str) -> list[str]:
    # PowerShell's Tee-Object writes UTF-16LE with a BOM by default; plain redirects are usually UTF-8.
    with open(log_path, "rb") as f:
        head = f.read(2)
    encoding = "utf-16" if head == b"\xff\xfe" else "utf-8"
    with open(log_path, "r", encoding=encoding, errors="replace") as f:
        return f.readlines()


def main() -> None:
    log_path, out_path = sys.argv[1], sys.argv[2]
    lines = read_lines(log_path)

    rows = []
    current = None
    for line in lines:
        line = line.rstrip("\n")
        m = BENCH_RE.match(line)
        if m:
            current = {"Method": m.group(1), "N": int(m.group(2))}
            continue
        if current is None:
            continue
        m = MEAN_RE.match(line)
        if m:
            current["MeanUs"] = to_us(m.group(1), m.group(2))
            current["StdDevUs"] = to_us(m.group(5), m.group(6))
            continue
        m = MINMAX_RE.match(line)
        if m:
            current["MinUs"] = to_us(m.group(1), m.group(2))
            current["MaxUs"] = to_us(m.group(3), m.group(4))
            rows.append(current)
            current = None
            continue

    def fmt(us: float) -> str:
        if us >= 1000:
            return f"{us / 1000:,.3f} ms"
        return f"{us:,.3f} us"

    header = "| Method | N | Mean | StdDev | Min | Max |\n"
    sep = "|---|---|---|---|---|---|\n"
    body_lines = []
    for r in rows:
        body_lines.append(
            f"| {r['Method']} | {r['N']} | {fmt(r['MeanUs'])} | {fmt(r['StdDevUs'])} | {fmt(r['MinUs'])} | {fmt(r['MaxUs'])} |"
        )

    with open(out_path, "w", encoding="utf-8") as f:
        f.write(f"# Reconstructed partial run ({len(rows)} benchmarks)\n\n")
        f.write(header)
        f.write(sep)
        f.write("\n".join(body_lines))
        f.write("\n")

    print(f"Parsed {len(rows)} benchmark results -> {out_path}")


if __name__ == "__main__":
    main()
