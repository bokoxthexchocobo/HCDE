#!/usr/bin/env python3
"""Clear legacy DoRadiusGive credit when landing action-do-radius-give-chunk579."""


def apply(tally: dict) -> None:
    for ch in tally.get("chunks", []):
        if ch.get("name") != "action-explode-radius-give":
            continue
        ch["nativeRanges"] = []
        ch["nativeLoc"] = 0
        ch["managedLoc"] = 0
        ch["retired"] = True
        note = ch.get("note") or ""
        if "chunk579" not in note:
            ch["note"] = (
                note
                + " Retired (2026-10-27): `DoRadiusGive`/`A_RadiusGive` re-landed as action-do-radius-give-chunk579."
            ).strip()
