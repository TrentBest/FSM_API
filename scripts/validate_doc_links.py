#!/usr/bin/env python3
"""Validate repository-relative links and image references in source Markdown.

External URLs and fenced code examples are intentionally ignored. Local links
may target either files or directories; this also supports GitHub folder links.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
MARKDOWN = [
    ROOT / "README.md",
    ROOT / "FSM_API" / "NuGetReadme.md",
    *sorted((ROOT / "Documentation").rglob("*.md")),
]
LINK_RE = re.compile(r"!?(?:\[[^\]]*\])\(\s*(?:<([^>]+)>|([^\s)]+))[^)]*\)")
HTML_REF_RE = re.compile(r"""\b(?:href|src)\s*=\s*["']([^"']+)["']""", re.IGNORECASE)
FENCE_RE = re.compile(r"^\s{0,3}(\x60{3,}|~{3,})")


def without_fenced_code(text: str) -> str:
    result: list[str] = []
    fence_char: str | None = None
    fence_length = 0
    for line in text.splitlines():
        match = FENCE_RE.match(line)
        if match:
            marker = match.group(1)
            if fence_char is None:
                fence_char, fence_length = marker[0], len(marker)
                continue
            if marker[0] == fence_char and len(marker) >= fence_length:
                fence_char, fence_length = None, 0
                continue
        if fence_char is None:
            result.append(line)
    return "\n".join(result)


def local_target(source: Path, destination: str) -> Path | None:
    destination = destination.strip()
    if not destination or destination.startswith("#"):
        return None
    parsed = urlsplit(destination)
    if parsed.scheme or parsed.netloc:
        return None
    raw_path = unquote(parsed.path)
    if not raw_path:
        return None
    if raw_path.startswith("/"):
        candidate = ROOT / raw_path.lstrip("/")
    else:
        candidate = source.parent / raw_path
    try:
        return candidate.resolve().relative_to(ROOT.resolve())
    except ValueError:
        return None


def main() -> int:
    missing: list[tuple[str, str]] = []
    checked = 0
    for source in MARKDOWN:
        if not source.is_file():
            continue
        text = without_fenced_code(source.read_text(encoding="utf-8-sig"))
        destinations = [
            (m.group(1) or m.group(2) or "").strip()
            for m in LINK_RE.finditer(text)
        ]
        destinations.extend(m.group(1).strip() for m in HTML_REF_RE.finditer(text))
        for destination in destinations:
            target = local_target(source, destination)
            if target is None:
                continue
            absolute = ROOT / target
            checked += 1
            if not absolute.exists():
                missing.append((source.relative_to(ROOT).as_posix(), destination))

    if missing:
        print(f"Found {len(missing)} broken local Markdown links/image references:")
        for source, destination in missing:
            print(f"  {source}: {destination}")
        return 1
    print(f"Documentation link check passed: {checked} local references checked.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
