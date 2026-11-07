# Chunk 1430 — S_AddPlayerGender through S_MarkPlayerSounds in s_advsound.cpp (1454-1759).

## Scope

S_AddPlayerGender through S_MarkPlayerSounds in s_advsound.cpp (1454-1759).

## Native ranges

- `src/sound/s_advsound.cpp` **S_AddPlayerGender** 1454–1759 (306 lines)

**Credited native LOC:** 306 (tally `nativeLoc` 306).

## Managed

`VmAdvSound1430Slice.cs`
Managed LOC (tally): 284.
Conversion commit: `74d7dfd70352b6f8ef2057557539a0bc802ce393`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Gender list and shrink (**1462–1498**) | yes |
| Player lookup (**1507–1571**) | yes |
| Save and restore (**1582–1608**) | yes |
| Equivalence and skin class (**1617–1759**) | yes |
| Band hull (**1454–1759**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| SNDINFO commands | already credited through line 1449, with holes under 150 lines left uncredited |
| soundlinks | starts at line 1767 |
| Add-player-sound bodies | callees |

Adding a gender reserves one hash table when that slot is empty and stores the index truncated to 16 bits. Shrinking sorts class names with ASCII case folding, marks the table sorted, and then resolves the default class. Lookup of a non-reserved sound returns that id. A missing gender slot uses the first defined gender, or the default class, or the empty sound when the class is already the default. After the table is sorted, an empty, missing, or silent-less unlinked lump falls back to male and then to the default class. A silent unlinked lump is kept. Saving appends a skin record. Restoring replays each record through the existing-alias or lump callee. Equal ids match before a validity test, so the empty id matches itself. Invalid ids do not match a different id. Alias links are followed. A random header stops. A player reserve is replaced with the skinned sound. The sound class is the skin name when the skin index sits at or above the class count and below the skin count, the skin is allowed, and the player class string is empty. Otherwise it is the player string, or the actor name, or player. Hexen actors without a player use fighter. Other games use player. The extended lookup tries name-extended and then name, case-insensitively, and then skins that id. Marking reads gender slot 0 on all four passes.

| Managed (`VmAdvSound1430Slice.cs`) | Native (`s_advsound.cpp`) |
|------------------------------------|--------------------------|
| Player lookup and skin class | **1454–1759** |
| Band hull (exclusive slice) | **1454–1759** |

### Chunk **1430** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **306** |
| **Net headline** | **+306** vs the prior chunk |


**Post-review ratio:** 284/306 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmAdvSound1430SliceTests.PlayerListsSortAndFallbacks`
- `VmAdvSound1430SliceTests.SkinsEquivalenceAndMarks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1430 --to 1430` (0 hard / 0 soft).

## Seams

Player-sound add and the already-credited class search stay callees. SNDINFO commands end before this band. soundlinks starts at line 1767.

## Credit rationale

UNCredited s_advsound.cpp 1454-1759 (+306). This is the player-class list, lookup, save, and skin comparison tail. The band is shorter than 400 lines and is not sliced.
