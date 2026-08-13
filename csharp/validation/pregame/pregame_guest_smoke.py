#!/usr/bin/env python3
"""Cross-language pregame guest smoke test (C# guest vs C++ hcdeserv)."""

from __future__ import annotations

import argparse
import os
import shutil
import subprocess
import sys
import time
from pathlib import Path


def repo_root() -> Path:
    return Path(__file__).resolve().parents[3]


def find_guest_cli(root: Path) -> list[str]:
    dotnet = shutil.which("dotnet")
    if dotnet is None:
        raise RuntimeError("dotnet SDK not found")
    project = root / "csharp" / "src" / "HCDE.PregameGuest.Cli" / "HCDE.PregameGuest.Cli.csproj"
    return [dotnet, "run", "--project", str(project), "--configuration", "Release", "--no-launch-profile", "--"]


def launch_server(args: argparse.Namespace) -> subprocess.Popen[str]:
    cmd = [
        str(args.server),
        "-dedicated",
        "-host",
        "1",
        "-iwad",
        str(args.iwad),
        "+map",
        args.map,
        "-port",
        str(args.game_port),
    ]
    return subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)


def stop_process(proc: subprocess.Popen[str]) -> str:
    proc.terminate()
    try:
        return proc.communicate(timeout=10)[0] or ""
    except subprocess.TimeoutExpired:
        proc.kill()
        return proc.communicate(timeout=10)[0] or ""


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--server", type=Path, default=Path("build/hcdeserv"))
    parser.add_argument("--iwad", type=Path, default=Path("doom2.wad"))
    parser.add_argument("--map", default="MAP01")
    parser.add_argument("--game-port", type=int, default=5029)
    parser.add_argument("--timeout-seconds", type=float, default=45.0)
    parser.add_argument("--wad-crc", action="append", default=[], help="Repeat for each required WAD CRC")
    parser.add_argument("--engine-version", default="1.0.0")
    args = parser.parse_args()

    root = repo_root()
    if not args.server.exists():
        print(f"[skip] server binary not found: {args.server}")
        return 0
    if not args.iwad.exists():
        print(f"[skip] IWAD not found: {args.iwad}")
        return 0

    guest_cmd = find_guest_cli(root)
    guest_cmd.extend(
        [
            "--server",
            f"127.0.0.1:{args.game_port}",
            "--engine-version",
            args.engine_version,
            "--timeout-ms",
            str(int(args.timeout_seconds * 1000)),
        ]
    )
    for crc in args.wad_crc:
        guest_cmd.extend(["--wad-crc", crc])

    print("[case] csharp-pregame-guest-vs-hcdeserv")
    server = launch_server(args)
    try:
        time.sleep(3.0)
        result = subprocess.run(guest_cmd, cwd=root, capture_output=True, text=True, timeout=args.timeout_seconds)
        print(result.stdout.strip())
        if result.returncode != 0:
            print(result.stderr.strip(), file=sys.stderr)
            output = stop_process(server)
            print(output[-4000:], file=sys.stderr)
            return result.returncode
    finally:
        if server.poll() is None:
            stop_process(server)

    print("[pass] csharp-pregame-guest-vs-hcdeserv")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
