# Chunk 691 — Whole `P_GetHealthGroup*`, serialize, and core ZScript destructible actions (`p_destructible.cpp` 220–237, 825–982).

## Scope

Whole `P_GetHealthGroup*`, serialize, and core ZScript destructible actions (`p_destructible.cpp` 220–237, 825–982).

## Native ranges

- `src/playsim/p_destructible.cpp` **P_GetHealthGroup** 220–224 (5 lines)
- `src/playsim/p_destructible.cpp` **P_GetHealthGroupOrNew** 226–237 (12 lines)
- `src/playsim/p_destructible.cpp` **Serialize** 825–834 (10 lines)
- `src/playsim/p_destructible.cpp` **P_SerializeHealthGroups** 836–868 (33 lines)
- `src/playsim/p_destructible.cpp` **ZScript destructible actions** 879–982 (104 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmPDestructibleSerializeScript.cs`
Managed LOC (tally): 102.
Conversion commit: `866b0df9a3977b3c6865a8279f3021777109d255`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `VmPDestructibleSerializeScript` | `P_GetHealthGroup` 220–224; `P_GetHealthGroupOrNew` 226–237; `Serialize` 825–834; `P_SerializeHealthGroups` 836–868; `FindHealthGroup`/`SetHealth`/`Damage*`/`Check*` DEFINE 879–982 |


**Post-review ratio:** 102/164 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleSerializeScriptTests.GetHealthGroupOrNew_CreatesWhenMissing`
- `VmPDestructibleSerializeScriptTests.ApplyReadGroup_UpdatesExistingGroupHealth`
- `VmPDestructibleSerializeScriptTests.ResolveLineHealth_UsesGroupWhenPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 691 --to 691` (0 hard / 0 soft).

## Seams

`_Line`/`_Sector` Get/SetHealth DEFINE bodies remain native; line/sector property chunks follow later.

## Credit rationale

Health-group lookup, save-game merge, and script damage entry points after chunk 690.
