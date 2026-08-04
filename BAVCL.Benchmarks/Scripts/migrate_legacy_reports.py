#!/usr/bin/env python3

"""Migrate legacy BenchmarkDotNet reports from Results/incoming/ into suite folders.



Converts flat `BAVCL.Benchmarks.{Suite}-report-github.md` files into:

  Results/{Suite}/{YYYY-MM-DD_HHMMSS}_{tag}.md



Processed originals are moved to Results/archive/.

"""



from __future__ import annotations



import re

import shutil

from datetime import datetime

from pathlib import Path



SCRIPT_DIR = Path(__file__).resolve().parent

PROJECT_DIR = SCRIPT_DIR.parent

RESULTS_DIR = PROJECT_DIR / "Results"

LEGACY_DIR = RESULTS_DIR / "incoming"

ARCHIVE_DIR = RESULTS_DIR / "archive"



LEGACY_NAME_RE = re.compile(

    r"^BAVCL\.Benchmarks\.(.+?)(?:-(?:report|after)-github)?\.md$"

)

MIGRATED_FROM_RE = re.compile(

    r"^\*\*Migrated from:\*\*\s*`(?:results|incoming)/(.+?)`\s*$", re.MULTILINE

)



# Explicit destination tags (filename stem suffix after timestamp).

DESTINATION_TAGS: dict[str, str] = {

    "BAVCL.Benchmarks.SortBenchmarks-report-github.md": "post-refactor-gpu-baseline",

}



# Exact duplicates — do not import (handled separately).

SKIP_IMPORT: set[str] = {

    "README.md",

}





def legacy_timestamp(path: Path) -> str:

    return datetime.fromtimestamp(path.stat().st_mtime).strftime("%Y-%m-%d_%H%M%S")





def destination_path(suite: str, path: Path) -> Path:

    ts = legacy_timestamp(path)

    if path.name in DESTINATION_TAGS:

        tag = DESTINATION_TAGS[path.name]

    elif ts.startswith("2026-07"):

        tag = "july-2026-baseline"

    else:

        tag = "legacy-baseline"

    return RESULTS_DIR / suite / f"{ts}-{tag}.md"





def find_existing_migration(source_name: str, suite: str) -> Path | None:

    suite_dir = RESULTS_DIR / suite

    if not suite_dir.exists():

        return None

    for candidate in suite_dir.glob("*.md"):

        text = candidate.read_text(encoding="utf-8")

        match = MIGRATED_FROM_RE.search(text)

        if match and match.group(1) == source_name:

            return candidate

    return None





def build_header(suite: str, dest: Path, source_name: str, note: str | None = None) -> str:

    match = re.match(r"^(\d{4}-\d{2}-\d{2}_\d{6})(?:[-_](.+))?$", dest.stem)

    ts = match.group(1) if match else dest.stem

    lines = [

        f"# BAVCL Benchmark Run — {suite}",

        "",

        f"**Suite:** {suite}  ",

        f"**Timestamp:** {ts}  ",

        f"**Migrated from:** `incoming/{source_name}`  ",

    ]

    if note:

        lines.append(f"**Note:** {note}  ")

    lines.append("")

    return "\n".join(lines)





def table_body(raw: str) -> str:

    """Return markdown body starting at the benchmark table or BDN header."""

    text = raw.lstrip()

    if text.startswith("# BAVCL Benchmark Run"):

        # Already partially converted — skip old header

        idx = text.find("```")

        if idx >= 0:

            return text[idx:]

    return text





def migrate_file(path: Path, *, dry_run: bool = False) -> str:

    if path.name in SKIP_IMPORT:

        return f"skip duplicate: {path.name}"



    match = LEGACY_NAME_RE.match(path.name)

    if not match:

        return f"skip unknown: {path.name}"



    suite = match.group(1)

    if path.name.endswith("-after-github.md"):

        return archive_only(path, reason="duplicate of report-github", dry_run=dry_run)



    existing = find_existing_migration(path.name, suite)

    if existing:

        return archive_only(

            path,

            reason=f"already migrated -> {existing.relative_to(PROJECT_DIR)}",

            dry_run=dry_run,

        )



    dest = destination_path(suite, path)

    if dest.exists():

        return archive_only(

            path,

            reason=f"destination exists: {dest.relative_to(PROJECT_DIR)}",

            dry_run=dry_run,

        )



    note = None

    if suite == "SortBenchmarks" and "post-refactor" in dest.stem:

        note = (

            "GPU sort/argsort baseline after Aug 2026 refactor "

            "(float radix, argsort 2D buffer reuse). Pre-trimmed N params (5 steps)."

        )



    header = build_header(suite, dest, path.name, note)

    content = header + table_body(path.read_text(encoding="utf-8"))



    if dry_run:

        return f"would migrate -> {dest.relative_to(PROJECT_DIR)}"



    dest.parent.mkdir(parents=True, exist_ok=True)

    dest.write_text(content, encoding="utf-8")

    archive_only(path, reason="migrated", dry_run=False)

    return f"migrated -> {dest.relative_to(PROJECT_DIR)}"





def archive_only(path: Path, *, reason: str, dry_run: bool) -> str:

    if dry_run:

        return f"would archive ({reason}): {path.name}"

    ARCHIVE_DIR.mkdir(parents=True, exist_ok=True)

    target = ARCHIVE_DIR / path.name

    if target.exists():

        target.unlink()

    shutil.move(str(path), str(target))

    return f"archived ({reason}): {path.name}"





def rename_plain_july_baselines(*, dry_run: bool = False) -> list[str]:

    """Add july-2026-baseline tag to earlier migrations that used bare timestamps."""

    messages: list[str] = []

    pattern = re.compile(r"^(\d{4}-\d{2}-\d{2}_\d{6})_july-2026-baseline\.md$")



    for path in RESULTS_DIR.rglob("*.md"):

        if path.parent in {LEGACY_DIR, ARCHIVE_DIR}:

            continue

        if path.parent.name.lower() == "plots":

            continue

        match = pattern.match(path.name)

        if not match:

            continue

        ts = match.group(1)

        if not ts.startswith("2026-07"):

            continue



        tagged = path.with_name(f"{ts}-july-2026-baseline.md")

        if tagged.exists():

            continue

        if dry_run:

            messages.append(f"would rename -> {tagged.relative_to(PROJECT_DIR)}")

            continue

        path.rename(tagged)

        messages.append(f"renamed -> {tagged.relative_to(PROJECT_DIR)}")



    return messages





def main() -> None:

    import argparse



    parser = argparse.ArgumentParser(description="Migrate legacy benchmark markdown reports.")

    parser.add_argument("--dry-run", action="store_true", help="Print actions without writing")

    args = parser.parse_args()



    if not LEGACY_DIR.exists():

        print("No Results/incoming/ folder found.")

        return



    for msg in rename_plain_july_baselines(dry_run=args.dry_run):

        print(msg)



    for path in sorted(LEGACY_DIR.glob("*.md")):

        print(migrate_file(path, dry_run=args.dry_run))



    remaining = list(LEGACY_DIR.glob("*.md"))

    remaining = [p for p in remaining if p.name != "README.md"]

    if not remaining:

        print("incoming/ cleared (sources in Results/archive/).")

    else:

        print(f"remaining in incoming/: {[p.name for p in remaining]}")





if __name__ == "__main__":

    main()

