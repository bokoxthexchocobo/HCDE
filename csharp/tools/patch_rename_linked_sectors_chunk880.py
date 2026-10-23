"""Rename partial linked-sectors tally row when landing chunk 880."""

OLD = "linked-sectors-move-chunk174"
NEW = "linked-sectors-chunk880"


def apply(tally: dict) -> None:
    for ch in tally.get("chunks", []):
        if ch.get("name") == OLD:
            ch["name"] = NEW
            return
