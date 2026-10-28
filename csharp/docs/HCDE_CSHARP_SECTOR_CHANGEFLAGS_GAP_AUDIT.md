# Sector change flags and gap inventory audit

## Scope

`LS_Sector_ChangeFlags` (54); gap tool false positives from `ExecuteSpecial` handlers.

## Implementation

- `SectorChangeFlagsSpecial.cs`, `LineSpecials` dispatch
- `generate_line_special_gap.py` scans `ExecuteSpecial` and `special == N`

## Tests

- `SectorChangeFlagsLoadedMapTests`

## Gap outcome

- Regenerated `line_special_gap.json`: **3** remaining gaps (polyobject / projectile intercept family).
