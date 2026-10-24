NAMES = {
    "p-action-attack-projectile-chunk743",
    "p-action-melee-rail-chunk744",
    "p-action-print-render-chunk745",
    "p-action-debris-particle-chunk746",
}


def apply(tally: dict) -> None:
    tally["chunks"] = [c for c in tally.get("chunks", []) if c.get("name") not in NAMES]
