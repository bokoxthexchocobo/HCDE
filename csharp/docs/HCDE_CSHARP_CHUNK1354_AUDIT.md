# Chunk 1354 — Game names, screen borders, VM field names, and the GAMEINFO key macros from gi.cpp (1-260).

## Scope

Game names, screen borders, VM field names, and the GAMEINFO key macros from gi.cpp (1-260).

## Native ranges

- `src/gamedata/gi.cpp` **GameNames** 1–260 (260 lines)

**Credited native LOC:** 260 (tally `nativeLoc` 260).

## Managed

`VmGameInfoKeys1354Slice.cs`
Managed LOC (tally): 379.
Conversion commit: `02feedfdf05e0c76af1863843066a327e01a3c08`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Game names and borders (**73–111**) | yes |
| VM field registrations (**39–71**) | yes |
| String and array key macros (**114–169**) | yes |
| Number, color, bool, font, and music macros (**171–258**) | yes |
| Band hull (**1–260**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| V_GetColorStringByName | color names stay outside this file |
| V_GetColorFromString | the color parse stays outside this file |
| S_FindSound | sound lookup stays outside this file |
| GetFileContainer | the lump container stays outside this file |
| ParseGameInfo | the key chain from **261** stays in chunk 206 |

GameNames has 17 slots. Doom, Heretic, Hexen, Strife, and Chex sit at 1, 2, 4, 8, and 16. Doom and Strife borders use offset 8 and size 8 with the brdr patches. Heretic uses offset 4 and size 16 with the bord patches. The border name list ends with a null slot. Field registrations use the script class AGameInfoStruct. The global name is gameinfo and the turbo cvar is declared here. Each key macro matches the lookup name without case. A fixed string longer than its limit reports the field name and does not store the value. A string array clears only when that flag is set, and a limit of 0 skips the length check. Sound entries store the lookup result. A stamped string also stores the container of the current lump. Float keys narrow to float. Double keys keep the wider value. Two doubles require a comma between the numbers. A known color name replaces the text before the color parse. An empty name leaves the text. False stores false. Any other token must be true. A font with no comma stores the color None. A patch stores the color Null. Music text after a colon is an atoi order, and the stored song stops at the colon.

| Managed (VmGameInfoKeys1354Slice.cs) | Native (gi.cpp) |
|--------------------------------------|-----------------|
| Names, borders, fields, and key macros | **1–260** |
| Band hull (exclusive slice) | **1–260** |

### Chunk **1354** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **260** |
| **Net headline** | **+260** vs the prior chunk |


**Post-review ratio:** 379/260 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmGameInfoKeys1354SliceTests.NamesBordersAndFields`
- `VmGameInfoKeys1354SliceTests.KeyMacros`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1354 --to 1354` (0 hard / 0 soft).

## Seams

V_GetColorStringByName, V_GetColorFromString, S_FindSound, and GetFileContainer stay injected. ParseGameInfo stays in chunk 206.

## Credit rationale

UNCredited gi.cpp 1-260 (+260). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of ArrayStateNode.cpp 1-259.
