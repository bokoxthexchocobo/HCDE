# Chunk 1287 — IWAD labels, addon joining, and the launcher layout from playgamepage.cpp (1-243).

## Scope

IWAD labels, addon joining, and the launcher layout from playgamepage.cpp (1-243).

## Native ranges

- `src/widgets/playgamepage.cpp` **PlayGamePage::PlayGamePage** 1–243 (243 lines)

**Credited native LOC:** 243 (tally `nativeLoc` 243).

## Managed

`VmPlayGame1287Slice.cs`
Managed LOC (tally): 293.
Conversion commit: `93eba1a77d52e9650cb59e2beec95d5d6ea4bce2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Wad labels (**31–77**) | yes |
| Values and language (**79–101**) | yes |
| Addon list (**103–196**) | yes |
| Layout (**198–243**) | yes |
| Band hull (**1–243**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| String table | host lookup |
| File dialog | host accept flag and names |

A path is split on a forward slash only, and a trailing slash keeps the wad name alone. The initial selection must be non-negative and below the wad count. Empty arguments leave the parameter text untouched. Carriage return is replaced before line feed, both become the pipe separator, and empty pieces are dropped before each piece is stripped. A byte at or above 128 is not a space. A later addon that matches an earlier one without regard to case is dropped, and the first spelling stays. Dropping a file returns true even when the list does not grow. A cancelled dialog does not refresh the edit. Both welcome placeholders are replaced. The addon row may sit at a negative x, and the game list height floors at zero.

| Managed (`VmPlayGame1287Slice.cs`) | Native (`playgamepage.cpp`) |
|------------------------------------|-----------------------------|
| Labels, addons, and the layout | **1–243** |
| Band hull (exclusive slice) | **1–243** |

### Chunk **1287** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **243** |
| **Net headline** | **+243** vs post-**1286** |


**Post-review ratio:** 293/243 managed/native ≈ 121% (src file LOC at HEAD; see tally).

## Tests

- `VmPlayGame1287SliceTests.Setup_FormatsTheWadLabelAndKeepsTheFirstAddon`
- `VmPlayGame1287SliceTests.Layout_ClampsTheAddonRowAndTheListHeight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1287 --to 1287` (0 hard / 0 soft).

## Seams

String lookup and the file dialog are host inputs. The edit control is the joined addon text.

## Credit rationale

UNCredited playgamepage.cpp 1-243 (+243). A full tally line scan shows no overlap. The file ends at line 243.
