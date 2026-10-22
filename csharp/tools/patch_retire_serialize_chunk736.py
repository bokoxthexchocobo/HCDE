"""Retire p-mobj-serialize-model-chunk736: partial port does not cover credited native band."""

CHUNK = "p-mobj-serialize-model-chunk736"


def apply(tally: dict) -> None:
    for ch in tally.get("chunks", []):
        if ch.get("name") != CHUNK:
            continue
        ch["retired"] = True
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["note"] = (
            "Retired: model serialize band (1749–1918) not faithfully ported; "
            "VmPMobjSerializeModel.cs kept uncredited."
        )
        return
    raise SystemExit(f"chunk not found: {CHUNK}")
