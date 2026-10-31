# Chunk 1360 — KEYCONF addkeysection, addmenukey, line comments, clearplayerclasses, and addplayerclass from keysections.cpp (78-323).

## Scope

KEYCONF addkeysection, addmenukey, line comments, clearplayerclasses, and addplayerclass from keysections.cpp (78-323).

## Native ranges

- `src/gamedata/keysections.cpp` **KeySections** 78–323 (246 lines)

**Credited native LOC:** 246 (tally `nativeLoc` 246).

## Managed

`VmKeySections1360Slice.cs`
Managed LOC (tally): 246.
Conversion commit: `b1ba33095fc78d02bb04a871328905ebe4ea01ae`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| CurrentKeySection (**78–79**) | yes |
| addkeysection (**81–115**) | yes |
| addmenukey (**117–138**) | yes |
| KEYCONF loader banner (**140–148**) | yes |
| D_LoadWadSettings (**150–237**) | yes |
| setslotstrict (**239–240**) | yes |
| ClearIWADPlayerClasses (**242–256**) | yes |
| clearplayerclasses (**258–279**) | yes |
| ValidatePlayerClass declaration (**281**) | yes |
| addplayerclass (**283–323**) | yes |
| Band hull (**78–323**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| LoadKeys (**39–55**) | the binding loader is already outside this hole; the call sites pass the original ini name |
| DoSaveKeys and M_SaveCustomKeys (**57–77**) | chunk 147 |
| VmKeyConfParser | an older looser reader; it is not this credit |

`addkeysection` runs only while `ParsingKeyConf` is set. `argc` must be 3, and `argv[0]` is the command. Any other count prints `Usage: addkeysection <menu section name> <ini name>` and returns. An ini name longer than 32 bytes prints the warning with the full name, then the search uses the first 32 bytes. The search compares that truncated ini name to `mTitle` with `CompareNoCase`. A match stores `CurrentKeySection` and does not call `LoadKeys`. A new section stores `mTitle` from `argv[1]` and `mSection` from the original `argv[2]`, then calls `LoadKeys` with that original name and 0, then the same name and 1. `CurrentKeySection` starts at -1.

`addmenukey` uses the same parsing gate and the same argc check, with `Usage: addmenukey <description> <command>`. `CurrentKeySection` of -1, or a current index at or past the section count, prints `You must use addkeysection first.` Index -2 is not that check, so it indexes the section array. A live section appends the description and the command with no trim.

`D_LoadWadSettings` sets `ParsingKeyConf`, clears key sections, then clears `KeyConfWeapons`. Each lump starts with the block comment closed. Newline skipping consumes every consecutive `\n` and `\r`, so a blank line in the middle disappears. A lump that starts with newlines submits one empty command and then the rest. A trailing newline does not add an empty command. An empty lump submits nothing. The comment filter does not trim. `//` pops the previous slash and stops the line. `/*` pops the slash, opens the block, and keeps scanning. `*/` closes the block and does not keep the star or the slash. Quotes toggle per line and protect both comment forms. The block flag survives later lines of the same lump and resets on the next lump. `http://x` becomes `http:`. The filtered text, including an empty line, is what `AddCommandString` receives. `CurrentKeySection` is left alone. `ParsingKeyConf` is cleared after the lumps.

`ClearIWADPlayerClasses` shifts a matching type left and pops, then the outer index still increments, so two adjacent matches keep one. A null type matches a null type. The last element pops with the inner loop skipped.

`clearplayerclasses` runs only while `ParsingKeyConf` is set. `setslotstrict` clears every class and does not search. Otherwise it searches DoomPlayer, HereticPlayer, StrifePlayer, FighterPlayer, ClericPlayer, MagePlayer, and ChexPlayer, in that order, and removes each result even when the search returns null.

`addplayerclass` runs only while `ParsingKeyConf` is set and `argc` is greater than 1. It searches, then validates. A failed validate does not read flags and does not push. An existing type pointer returns before flags, so an unknown flag is not printed and the class is not pushed again. `nomenu` in any case sets `PCF_NOMENU` (1). Any other flag prints `Unknown flag '%s' for player class '%s'` with the original class name, and the class is still pushed.

| Managed (VmKeySections1360Slice.cs) | Native (keysections.cpp) |
|-------------------------------------|--------------------------|
| Section commands through addplayerclass | **78–323** |
| Band hull (exclusive slice) | **78–323** |

### Chunk **1360** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **246** |
| **Net headline** | **+246** vs the prior chunk |


**Post-review ratio:** 246/246 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmKeySections1360SliceTests.CommandsAndPlayerClasses`
- `VmKeySections1360SliceTests.KeyConfLines`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1360 --to 1360` (0 hard / 0 soft).

## Seams

LoadKeys is defined at lines 39-55 and stays a hook. FindActor and ValidatePlayerClass are declared outside this band. AddCommandString receives the filtered line; this slice records that string.

## Credit rationale

UNCredited keysections.cpp 78-323 (+246). A full tally line scan, including recognition ranges, shows no overlap. This ties i_input_gyro.cpp 1-246. Path order takes the key section file. Gaps longer than 400 lines stay whole and are not sliced.
