# Chunk 686 — Whole `ChangePortalLine`, `ChangePortal`, `P_GetLineSide`, `P_ClipLineToPortal`, and `P_TranslatePortal*` (`portal.cpp` 330–524).

## Scope

Whole `ChangePortalLine`, `ChangePortal`, `P_GetLineSide`, `P_ClipLineToPortal`, and `P_TranslatePortal*` (`portal.cpp` 330–524).

## Native ranges

- `src/playsim/portal.cpp` **FLevelLocals::ChangePortalLine** 330–362 (33 lines)
- `src/playsim/portal.cpp` **FLevelLocals::ChangePortal** 371–383 (13 lines)
- `src/playsim/portal.cpp` **P_GetLineSide** 391–395 (5 lines)
- `src/playsim/portal.cpp` **P_ClipLineToPortal** 397–433 (37 lines)
- `src/playsim/portal.cpp` **P_TranslatePortalXY** 441–461 (21 lines)
- `src/playsim/portal.cpp` **P_TranslatePortalVXVY** 469–480 (12 lines)
- `src/playsim/portal.cpp` **P_TranslatePortalAngle** 488–495 (8 lines)
- `src/playsim/portal.cpp` **P_TranslatePortalZ** 503–524 (22 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmPortalChangePortal.cs`, `VmPortalClipTranslate.cs`
Managed LOC (tally): 199.
Conversion commit: `d8d4e1e9a304647632fa43e6424e33fce0b63964`.

## Padding review (2026-10-21)

| Managed | Native (`portal.cpp`) |
|---------|---------------------|
| `VmPortalChangePortal` | `ChangePortalLine` 330–362; `ChangePortal` 371–383 |
| `VmPortalClipTranslate` | `P_GetLineSide` 391–395; `P_ClipLineToPortal` 397–433; `P_TranslatePortalXY` 441–461; `P_TranslatePortalVXVY` 469–480; `P_TranslatePortalAngle` 488–495; `P_TranslatePortalZ` 503–524 |

### Out-of-body uncredit (686)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **151** |
| **Out-of-body removed** | **0** |
| **Honest** | **151** |


**Post-review ratio:** 199/151 managed/native ≈ 132% (src file LOC at HEAD; see tally).

## Tests

- `VmPortalChangePortalTests.ChangePortalLine_RejectsLinked`
- `VmPortalClipTranslateTests.ClipLineToPortal_RejectsLineBehindPortal`
- `VmPortalClipTranslateTests.TranslatePortalXy_RotatesAndOffsets`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 686 --to 686` (0 hard / 0 soft).

## Seams

`getPortalDestination`, `DAngle`, and VM action wrappers remain native.

## Credit rationale

Portal destination edits and coordinate translation after blockmap chunk 685.
