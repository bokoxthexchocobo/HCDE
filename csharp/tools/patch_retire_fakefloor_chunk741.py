"""Retire p-mobj-fakefloor-inventory-chunk741: overlapped 737/738 and used banned wrapper glue."""

CHUNK = "p-mobj-fakefloor-inventory-chunk741"


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
            "Retired: duplicated Find/Give inventory credit and DEFINE wrappers; "
            "fake-floor port deferred for an honest non-overlapping chunk."
        )
        return
    raise SystemExit(f"chunk not found: {CHUNK}")
