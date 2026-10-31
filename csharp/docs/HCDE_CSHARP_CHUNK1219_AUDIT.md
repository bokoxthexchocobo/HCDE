# Chunk 1219 — Button slots, name lookup, and axis clamp from c_buttons.cpp (1-316).

## Scope

Button slots, name lookup, and axis clamp from c_buttons.cpp (1-316).

## Native ranges

- `src/common/console/c_buttons.cpp` **FButtonStatus::PressKey** 1–316 (316 lines)

**Credited native LOC:** 316 (tally `nativeLoc` 316).

## Managed

`VmButtons1219Slice.cs`
Managed LOC (tally): 248.
Conversion commit: `158fe54b0add8982d46b97836d93e572de7cb709`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SetButtons (**40–51**) | yes |
| ListActionCommands (**59–80**) | yes |
| FindButtonIndex (**89–100**) | yes |
| Reset triggers and states (**109–133**) | yes |
| GetAxes (**141–153**) | yes |
| PressKey (**161–199**) | yes |
| ReleaseKey (**207–249**) | yes |
| AddAxes (**257–296**) | yes |
| AddButtonTabCommands (**304–316**) | yes |
| Band hull (**1–316**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

The downward scan keeps the lowest free slot. A repeated key does not change the button. The seventh key prints the overflow line and leaves the slots unchanged. Releasing one of several keys sets the up flag and leaves the button down. The axis sum uses only values greater than zero, then clamps to the unit interval with a comparison that leaves a NaN unchanged.

| Managed (`VmButtons1219Slice.cs`) | Native (`c_buttons.cpp`) |
|------------------------------------|-------------------------|
| Button map | **1–316** |
| Band hull (exclusive slice) | **1–316** |

### Chunk **1219** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **316** |
| **Net headline** | **+316** vs post-**1218** |


**Post-review ratio:** 248/316 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmButtons1219SliceTests.PressKey_FillsTheLowestSlotAndMasksTheDoubleClickBit`
- `VmButtons1219SliceTests.FindAndAxes_FoldNamesAndClampTheAxis`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1219 --to 1219` (0 hard / 0 soft).

## Seams

The double-click bit is masked with 0x7FFF. Key 0 is the console force path. FName lookup is ASCII case-fold and no-create, so None and an unknown name both return -1. Duplicate names keep the later index. CheckWildcards, key bindings, and the axis table are caller-supplied. The 32-byte matcher keeps 31 characters while the printed command uses the full name. A name longer than 14 characters overflows the native 16-byte strcpy and the port passes the full name.

## Credit rationale

UNCredited c_buttons.cpp 1-316 (+316). The file ends at 316. A full tally line scan shows no overlap.
