# Chunk 794 — `DDoor::Construct` overload and `FLevelLocals::EV_DoDoor` (`mapthinkers/a_doors.cpp` 355–538). Retired (2026-10-23): out-of-body credit removed; honest **75** < **150**.

## Scope

`DDoor::Construct` overload and `FLevelLocals::EV_DoDoor` (`mapthinkers/a_doors.cpp` 355–538). Retired (2026-10-23): out-of-body credit removed; honest **75** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `d0a2db2c48623fdc9b994eff070936aa78aa38cf`.

## Padding review (2026-10-21)

| Managed | Native (`a_doors.cpp`) |
|---------|-------------------------|
| `Construct` / `DoorThinker` fields | **355–367** |
| `Construct` compat light | **368–371** |
| `Construct` `doorClose` | **375–380** |
| `Construct` `doorOpen`/`doorRaise` | **382–389** |
| `Construct` `doorCloseWaitOpen` | **391–395** |
| `Construct` `doorWaitRaise` | **397–401** |
| `Construct` `doorWaitClose` | **403–410** |
| `Construct` bot dist block | **414–428** |
| `Construct` tail / `Super::Construct` | **429** |
| `EvDoDoor` signature / locals | **438–444** |
| `EvDoDoor` keys/manual/reuse/tag/diag | **445–536** |
| `EvDoDoor` return | **537–538** |

### Out-of-body uncredit (794)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **176** |
| **Out-of-body removed** | **101** |
| **Honest** | **75** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 794 --to 794` (0 hard / 0 soft).

## Seams

`P_CheckKeys`, sector tag iteration, `CreateThinker`, height finders, and diag hooks are injected on construct/EV contexts; `DoorSound` reuses `VmADoorTickSound`.

## Credit rationale

Vertical-door spawn and EV band after tick/sound chunk793.
