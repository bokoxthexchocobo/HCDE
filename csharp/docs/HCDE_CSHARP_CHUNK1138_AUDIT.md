# Chunk 1138 — Trigger and lock probes and the wall pass through the Boom secret color from `am_map.cpp` (**2551–2700**); follows **1137**.

## Scope

Trigger and lock probes and the wall pass through the Boom secret color from `am_map.cpp` (**2551–2700**); follows **1137**.

## Native ranges

- `src/am_map.cpp` **Automap wall pass** 2551–2700 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapWallPass1138Slice.cs`
Managed LOC (tally): 200.
Conversion commit: `09cb461a4fa1e4312bd8e87ff33842e7e2a86173`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Exit-secret arm (**2551**) | yes |
| `AM_isTriggerBoundary` (**2555–2573**) | yes |
| `AM_isLockBoundary` (**2576–2621**) | yes |
| Portal passes, offsets, rotation (**2631–2677**) | yes |
| Mapped, dontdraw, style, portal, Boom secret (**2679–2700**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2401–2550** | floor boundary already credited by **1137** |
| **2701+** | secret-sector draw and the rest of `drawWalls` |

`am_showtriggerlines == 1` hides door specials **10**, **11**, **12**, **14**, **202**, and **249**. Lock args: `FS_Execute` **158** uses `args[2]`; `Door_Animated` **14** and `Door_LockedRaise` **13** use `args[3]`; other locks use `args[4]`. A non-zero `locknumber` wins before the special probe. `ML_DONTDRAW` is **0x80**, `ML_MAPPED` is **0x100**, `WALLF_POLYOBJ` is **64**, and `AMLS_COUNT` is **14**. Style and dontdraw suppression apply when `am_cheat` is **0** or at least **4**.

| Managed (`VmAmMapWallPass1138Slice.cs`) | Native (`am_map.cpp`) |
|------------------------------------------|---------------------|
| Triggers, locks, wall pass | **2551–2700** |
| Band hull (exclusive slice) | **2551–2700** |

### Chunk **1138** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1137** |


**Post-review ratio:** 200/150 managed/native ≈ 133% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapWallPass1138SliceTests.Triggers_HideUnknownSpecialsAndPlainDoorsInModeOne`
- `VmAmMapWallPass1138SliceTests.LockBoundary_UsesTheLineLockOrTheSpecialArgument`
- `VmAmMapWallPass1138SliceTests.PortalPasses_SkipTheMapGroupAndPlaceTheOffset`
- `VmAmMapWallPass1138SliceTests.ChooseWall_StopsAtStylePortalOrBoomSecret`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1138 --to 1138` (0 hard / 0 soft).

## Seams

Special-info lookup, cheat-mode, and displacement offsets stay host-supplied. Unexplored-secret coloring begins at **2705**.

## Credit rationale

UNCredited **`am_map.cpp`** **2551–2700** (+150), inside the 3594-line file, with no conversion or recognition overlap.
