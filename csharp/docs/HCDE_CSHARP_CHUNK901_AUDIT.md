# Chunk 901 — `LS_Ceiling_LowerAndCrush` through `LS_Generic_Ceiling` (`p_lnspec.cpp` **701–913**). Follows **900** (**≤700**); stops before `LS_Generic_Crusher` (**915+**). Trimmed past **900** to complete generic-ceiling function.

## Scope

`LS_Ceiling_LowerAndCrush` through `LS_Generic_Ceiling` (`p_lnspec.cpp` **701–913**). Follows **900** (**≤700**); stops before `LS_Generic_Crusher` (**915+**). Trimmed past **900** to complete generic-ceiling function.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Ceiling_LowerAndCrush** 701–913 (213 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmPLnspecCeilingMoversSlice.cs`
Managed LOC (tally): 229.
Conversion commit: `cb171a266f7ae072d722bf19b51a0d5c00c70c95`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| Lower/crush + crush-stop | **701–729** |
| Crush-raise / move-to-value / instant | **731–774** |
| Crush A/dist/silent variants | **776–810** |
| Raise/lower/to-instant movers | **812–876** |
| `CeilingStop` / `GenericCeiling` | **878–913** |

### vs **900**

| | Lines |
|--|------:|
| **900** | **472–700** |
| **901** (this row) | **701–913** (**213**) |
| `LS_Generic_Crusher` | **915+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **213** |
| Net behavior-ported | **213** |


**Post-review ratio:** 229/213 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecCeilingMoversSliceTests.CeilingCrushStop_HexenDefaultRemoves`
- `VmPLnspecCeilingMoversSliceTests.CeilingMoveToValueTimes8_NegativeHeight`
- `VmPLnspecCeilingMoversSliceTests.GenericCeiling_RaiseTargetSelectsHighest`
- `VmPLnspecCeilingMoversSliceTests.GenericCeiling_CrushFlagSetsTwenty`
- `VmPLnspecCeilingMoversSliceTests.CeilingCrushAndRaiseDist_UsesDistAsHeight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 901 --to 901` (0 hard / 0 soft).

## Seams

`EV_DoCeiling`, `EV_CeilingCrushStop`, and `EV_StopCeiling` are not hosted.

## Credit rationale

Ceiling crush-stop gametype gate, move-to-value sign, crush-type slowdown flags, silent crush variants, instant movers, and generalized ceiling target switch via `VmPLnspecCeilingMoversSlice`.
