#!/usr/bin/env python3
"""Remove padding-shaped methods: delete uncalled; inline identity/constant at call sites."""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
REPO = TOOLS.parent.parent
CSHARP = TOOLS.parent
sys.path.insert(0, str(TOOLS))
from reaudit_chunks import _param_names, scan_padding_methods  # noqa: E402


def class_name_from_file(cs_text: str, path: Path) -> str:
    m = re.search(r"\b(?:public\s+)?(?:static\s+)?class\s+(\w+)", cs_text)
    return m.group(1) if m else path.stem


def find_method_def(cs_text: str, name: str) -> re.Match[str] | None:
    expr = re.compile(
        rf"(?m)^[ \t]*((?:public|private|internal|protected)[^\n]*\b{name}\s*\([^)]*\)\s*=>\s*[^;]+;)\s*$",
    )
    return expr.search(cs_text)


def _is_method_def_line(line: str, name: str) -> bool:
    if name not in line:
        return False
    if re.search(rf"\b{re.escape(name)}\s*\([^)]*\)\s*=>", line):
        return True
    return bool(
        re.search(
            rf"^\s*(?:public|private|internal|protected|static).*\b{re.escape(name)}\s*\(",
            line,
        )
    )


def count_calls_in_file(text: str, class_name: str, name: str) -> int:
    qual = re.compile(rf"\b{re.escape(class_name)}\.{re.escape(name)}\s*\(")
    unqual = re.compile(rf"(?<!\.){re.escape(name)}\s*\(")
    n = 0
    for line in text.splitlines():
        if _is_method_def_line(line, name):
            continue
        n += len(qual.findall(line))
        n += len(unqual.findall(line))
    return n


def count_calls_all(paths: list[Path], def_path: Path, class_name: str, name: str) -> int:
    n = 0
    for p in paths:
        t = p.read_text(errors="replace")
        if p == def_path:
            n += count_calls_in_file(t, class_name, name)
        else:
            qual = re.compile(rf"\b{re.escape(class_name)}\.{re.escape(name)}\s*\(")
            for line in t.splitlines():
                n += len(qual.findall(line))
    return n


def inline_calls_in_text(
    text: str,
    class_name: str,
    name: str,
    kind: str,
    detail: str,
    params: str,
    *,
    same_file: bool,
) -> str:
    pnames = _param_names(params)
    if kind == "constant_expr":
        patterns = [re.compile(rf"\b{re.escape(class_name)}\.{re.escape(name)}\s*\(\s*\)")]
        if same_file:
            patterns.append(re.compile(rf"(?<!\.){re.escape(name)}\s*\(\s*\)"))

        def repl_line(line: str) -> str:
            if _is_method_def_line(line, name):
                return line
            out = line
            for call in patterns:
                out = call.sub(detail, out)
            return out

        return "\n".join(repl_line(ln) for ln in text.splitlines()) + (
            "\n" if text.endswith("\n") else ""
        )
    if kind == "identity_expr" and len(pnames) == 1:
        patterns = [
            re.compile(
                rf"\b{re.escape(class_name)}\.{re.escape(name)}\s*\(\s*([^)]+?)\s*\)",
            ),
        ]
        if same_file:
            patterns.append(
                re.compile(rf"(?<!\.){re.escape(name)}\s*\(\s*([^)]+?)\s*\)"),
            )

        def repl_line(line: str) -> str:
            if _is_method_def_line(line, name):
                return line
            out = line

            def sub(m: re.Match[str]) -> str:
                arg = m.group(1).strip()
                if re.match(r"\w+\s*:", arg):
                    arg = arg.split(":", 1)[1].strip()
                if re.search(r"[?:|&]", arg):
                    return f"({arg})"
                return arg

            for call in patterns:
                out = call.sub(sub, out)
            return out

        return "\n".join(repl_line(ln) for ln in text.splitlines()) + (
            "\n" if text.endswith("\n") else ""
        )
    return text


def remove_def_line(cs_text: str, name: str) -> str:
    expr = re.compile(
        rf"(?m)^[ \t]*(?:public|private|internal|protected)[^\n]*\b{name}\s*\([^)]*\)\s*=>\s*[^;]+;\s*\n",
    )
    out, n = expr.subn("", cs_text)
    if n:
        return out
    block = re.compile(
        rf"(?m)^[ \t]*(?:public|private|internal|protected)[^\n]*\b{name}\s*\([^)]*\)\s*\{{[^{{}}]*\}}\s*\n",
    )
    return block.sub("", out)


def load_corpus() -> tuple[list[Path], str]:
    paths = list(CSHARP.rglob("*.cs"))
    return paths, "".join(p.read_text(errors="replace") for p in paths)


def process_file(rel: str, dry_run: bool) -> dict[str, int]:
    path = REPO / rel
    stats = {"deleted": 0, "inlined": 0, "skipped": 0}
    if not path.is_file():
        return stats

    while True:
        cs_text = path.read_text(errors="replace")
        class_name = class_name_from_file(cs_text, path)
        hits = scan_padding_methods(cs_text)
        if not hits:
            break
        progressed = False
        for hit in sorted(hits, key=lambda h: len(h["method"]), reverse=True):
            name = hit["method"]
            if hit["kind"] in ("plan_record_struct", "plan_aggregator_method"):
                continue
            mdef = find_method_def(cs_text, name)
            if not mdef:
                continue
            def_line = mdef.group(1)
            pm = re.search(rf"\b{name}\s*\(([^)]*)\)", def_line)
            params = pm.group(1) if pm else ""
            paths, _ = load_corpus()
            uses = count_calls_all(paths, path, class_name, name)
            if uses == 0:
                new_text = remove_def_line(cs_text, name)
                if new_text != cs_text:
                    stats["deleted"] += 1
                    progressed = True
                    if not dry_run:
                        path.write_text(new_text)
                    cs_text = new_text
                break
            if hit["kind"] not in ("identity_expr", "constant_expr"):
                stats["skipped"] += 1
                continue
            paths, _ = load_corpus()
            changed_any = False
            for p in paths:
                t = p.read_text(errors="replace")
                nt = inline_calls_in_text(
                    t,
                    class_name,
                    name,
                    hit["kind"],
                    hit["detail"],
                    params,
                    same_file=(p == path),
                )
                if nt != t:
                    changed_any = True
                    if not dry_run:
                        p.write_text(nt)
            if changed_any:
                cs_text = path.read_text(errors="replace")
                new_text = remove_def_line(cs_text, name)
                stats["inlined"] += 1
                progressed = True
                if not dry_run:
                    path.write_text(new_text)
                break
            stats["skipped"] += 1
        if not progressed:
            break

    return stats


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+", help="paths like csharp/src/HCDE.Playsim/VmFoo.cs")
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--passes", type=int, default=3, help="repeat until no progress")
    args = ap.parse_args()
    rels = [f if f.startswith("csharp/") else f"csharp/src/{f}" for f in args.files]
    tot = {"deleted": 0, "inlined": 0, "skipped": 0}
    for pass_i in range(args.passes):
        progressed = False
        for rel in rels:
            st = process_file(rel, args.dry_run)
            if st["deleted"] or st["inlined"]:
                progressed = True
            if pass_i == 0 or st["deleted"] or st["inlined"]:
                print(f"{rel}: {st}")
            for k in tot:
                tot[k] += st[k]
        if not progressed:
            break
    print("total:", tot)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
