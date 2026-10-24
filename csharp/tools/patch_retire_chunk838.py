"""Retire fscript-parser-sf-mid-chunk838: honest native credit 105 < 150."""

CHUNK = "fscript-parser-sf-mid-chunk838"


def apply(tally: dict) -> None:
    for ch in tally.get("chunks", []):
        if ch.get("name") != CHUNK:
            continue
        ch["retired"] = True
        ch["nativeRanges"] = []
        ch["managedSourceFiles"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch.pop("conversionManagedCommit", None)
        return
    raise SystemExit(f"chunk not found: {CHUNK}")


if __name__ == "__main__":
    import json
    from pathlib import Path

    p = Path(__file__).resolve().parents[1] / "docs/data/loc_tally.json"
    tally = json.loads(p.read_text(encoding="utf-8"))
    apply(tally)
    p.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
    print(f"retired {CHUNK}")
