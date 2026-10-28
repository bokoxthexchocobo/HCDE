# Chunk 691 — Whole `P_GetHealthGroup*`, serialize, and core ZScript destructible actions (`p_destructible.cpp` 220–237, 825–982). Retired (2026-10-24): `--band` hull overlaps existing credit (p-destructible-geometry-radius-chunk661 src/playsim/p_destructible.cpp 442-688).

## Scope

Whole `P_GetHealthGroup*`, serialize, and core ZScript destructible actions (`p_destructible.cpp` 220–237, 825–982). Retired (2026-10-24): `--band` hull overlaps existing credit (p-destructible-geometry-radius-chunk661 src/playsim/p_destructible.cpp 442-688).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4ca815a9075d2a892f51e589336ffc7355265fbc`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `VmPDestructibleSerializeScript` | `P_GetHealthGroup` 220–224; `P_GetHealthGroupOrNew` 226–237; `Serialize` 825–834; `P_SerializeHealthGroups` 836–868; `FindHealthGroup`/`SetHealth`/`Damage*`/`Check*` DEFINE 879–982 |

### Out-of-body uncredit (691)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **164** |
| **Out-of-body removed** | **9** |
| **Honest** | **155** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 691 --to 691` (0 hard / 0 soft).

## Seams

`_Line`/`_Sector` Get/SetHealth DEFINE bodies remain native; line/sector property chunks follow later.

## Credit rationale

Health-group lookup, save-game merge, and script damage entry points after chunk 690.
