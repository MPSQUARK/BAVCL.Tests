import re

log_path = r"C:\Users\marce\.cursor\projects\c-Users-marce-Repos-BAVCL\agent-tools\64047f78-0b3d-4b52-a70c-e34f2df6bc8b.txt"
lines = open(log_path, encoding="utf-8", errors="replace").readlines()
bench_re = re.compile(r"^// Benchmark: SortBenchmarks\.(\w+): .*\[N=(\d+)\]$")
found = []
for l in lines:
    m = bench_re.match(l.rstrip("\n"))
    if m:
        found.append((m.group(1), m.group(2)))
print(len(found))

mean_re = re.compile(
    r"^Mean = ([\d.]+) (us|ms|ns), StdErr = ([\d.]+) (us|ms|ns) \([\d.]+%\), N = \d+, StdDev = ([\d.]+) (us|ms|ns)$"
)
minmax_re = re.compile(
    r"^Min = ([\d.]+) (us|ms|ns), Q1 = [\d.]+ \w+, Median = [\d.]+ \w+, Q3 = [\d.]+ \w+, Max = ([\d.]+) (us|ms|ns)$"
)

current = None
completed = []
for l in lines:
    line = l.rstrip("\n")
    m = bench_re.match(line)
    if m:
        current = (m.group(1), m.group(2))
        continue
    if current is None:
        continue
    if mean_re.match(line):
        continue
    if minmax_re.match(line):
        completed.append(current)
        current = None

print("completed:", len(completed))
missing = [f for f in found if f not in completed]
print("found but not completed:", missing)
