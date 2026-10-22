CHUNK = "p-action-drop-burst-respawn-chunk747"


def apply(tally: dict) -> None:
    tally["chunks"] = [ch for ch in tally.get("chunks", []) if ch.get("name") != CHUNK]
