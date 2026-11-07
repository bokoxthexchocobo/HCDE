# Chunk 1429 — S_CheckSound through S_AddLocalSndInfo in s_advsound.cpp (251-644).

## Scope

S_CheckSound through S_AddLocalSndInfo in s_advsound.cpp (251-644).

## Native ranges

- `src/sound/s_advsound.cpp` **S_CheckSound** 251–644 (394 lines)

**Credited native LOC:** 394 (tally `nativeLoc` 394).

## Managed

`VmAdvSound1429Slice.cs`
Managed LOC (tally): 374.
Conversion commit: `e61352530e06a6ea46d7c3ad37cd574dd92b9965`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Recursive sound check (**258–343**) | yes |
| Millisecond length (**355–400**) | yes |
| Add sound (**410–462**) | yes |
| Player sound adds (**472–544**) | yes |
| Clear, parse, and local SNDINFO (**555–644**) | yes |
| Band hull (**251–644**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Declarations and command names | end at line 250 |
| Include paths | start at line 657 |
| Class, gender, lookup, skin, and parser bodies | callees |

A player-reserved sound is not walked. A sound already on the chain is accepted without a new report. A random choice that returns to the sound where the walk started prints the red random-recursion line and the orange chain from the second entry. An alias that returns to that sound prints the alias line, shrinks the chain back to its size at entry, and then pops one more entry. An empty pop does nothing. Integrity checks every sound on one shared chain, disables a failed sound by clearing its random header and linking it to the empty sound, and then readies rumble. Length follows one player-reserved skin or one plain alias and then loads that sound. A random header returns the longest choice and does not load itself. A missing sound is zero. A null load is zero. Missing sound data crashes. The action length divides milliseconds by 1000. Adding by lump name looks in the sounds namespace and drops any script pointer. An unknown name appends a sound with the current pitch mask. A known reserved sound stops with the reserved-player message, as a script error only when a scanner was passed. A compatibility sound rewrites its link target. A random definition is emptied and its owner becomes the empty sound. A near limit of -1 becomes 2 with a range of 65536. A player sound name is the class, a quoted gender digit, and the referenced name. The referenced link is stored before the new lump is added. The class and gender callees run after that add. A null reference or the empty sound returns the empty id without those callees. An existing alias registers the class and gender before that empty-id return, stores the referenced link, and saves the alias index when it comes from a skin. A duplicate player sound looks up the alias and then adds that existing id. Clearing wipes engine sounds only when an engine exists, and always clears the ambient, music, player, alias, device, and rumble tables. Parsing returns immediately when a redefine is requested and SNDINFO is locked. Otherwise it clears saved skins and the tables, forces pitch mask 0, adds { no sound } from DSEMPTY, records global SNDINFO lumps and every Strife voice lump, restores skins, hashes, shrinks, and checks integrity. Locking sets the lock. A locked local lump prints the DSDHacked sentence and returns. An unlocked local lump parses that lump, hashes, shrinks, and checks integrity.

| Managed (`VmAdvSound1429Slice.cs`) | Native (`s_advsound.cpp`) |
|------------------------------------|--------------------------|
| Sound table setup | **251–644** |
| Band hull (exclusive slice) | **251–644** |

### Chunk **1429** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **394** |
| **Net headline** | **+394** vs the prior chunk |


**Post-review ratio:** 374/394 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmAdvSound1429SliceTests.CyclesAndLengths`
- `VmAdvSound1429SliceTests.AddsClearsAndParse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1429 --to 1429` (0 hard / 0 soft).

## Seams

Random-list, skinned-sound, player-class, lump, and SNDINFO parser calls stay callees. Declarations end at line 250. Include paths start at line 657.

## Credit rationale

UNCredited s_advsound.cpp 251-644 (+394). This is the sound-table setup after the declaration block and stops before include-path resolution. The band is shorter than 400 lines and is not sliced.
