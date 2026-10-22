CHUNK = "p-action-drop-burst-respawn-chunk747"


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
        ch["note"] = "Retired: overlapped recognition chunk576; replaced by p-action-raise-flags-chunk747."
        return
