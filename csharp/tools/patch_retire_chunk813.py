"""Retire p-pspr-header-buttons-chunk813: credited non-function header/DEFINE bands only."""

CHUNK = "p-pspr-header-buttons-chunk813"


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
        ch["note"] = (
            "Retired: nativeRanges credited license/includes/CVAR/DEFINE_FIELD bands with "
            "zero whole-function bodies in p_pspr.cpp 1–162; adjacent psprite functions on chunk 765."
        )
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
