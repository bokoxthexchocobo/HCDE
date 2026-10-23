# Chunk 839 — `FParser::SF_PlayerName` … `SF_Spawn` from `fragglescript/t_func.cpp` (722–876).

## Scope

`FParser::SF_PlayerName` … `SF_Spawn` from `fragglescript/t_func.cpp` (722–876).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerName** 722–745 (24 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PlayerObj** 753–776 (24 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Player** 784–807 (24 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Spawn** 817–869 (53 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF player/spawn separators** 746–752 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF player/spawn separators** 777–783 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF player/spawn separators** 808–816 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF player/spawn separators** 870–876 (7 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmFScriptParserSfPlayer.cs`
Managed LOC (tally): 108.
Conversion commit: `f0effdb29f5c7fe06361184159880eca09bf5915`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp`) |
|---------|-------------------------|
| `ResolveOptionalPlayerNum` | **722–745**, **753–776** (optional-arg player resolution) |
| `SfPlayerName` | **722–745** |
| `TriggerPlayer` | **722–745**, **753–776** |
| `PlayerDisplayName` | **722–745** |
| `SfPlayerObj` | **753–776** |
| `SfPlayer` | **784–807** |
| `SfSpawn` | **817–869** |
| `FloatValue` | **817–869** |
| `SpawnActor` | **817–869** |
| `SetActorYaw` | **817–869** |
| `TestMobjLocation` | **857–866** |
| `DestroyFailedSpawn` | **861–865** |
| `SectorFloorZ` | **834–836** |
| `OnFloorZ` | **841–842** |
| `FsNoCheckPosition` | **857–858** |
| (native separators) | **746–752**, **777–783**, **808–816**, **870–876** |

**Post-review ratio:** 108/155 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfPlayerTests.SfPlayerName_ValidPlayer_ReturnsName`
- `VmFScriptParserSfPlayerTests.SfPlayer_FromMobj_ReturnsPlayerNum`
- `VmFScriptParserSfPlayerTests.SfSpawn_FailedLocation_ClearsReturn`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 839 --to 839` (0 hard / 0 soft).

## Seams

Player display name, spawn, placement test, and failed-spawn destroy are host callbacks on `ParserContext` (`VmFScriptParserSfPlayer.cs` partial only).

## Credit rationale

Whole-function band after mid SF messaging (**838**) through `SF_Spawn` before `SF_RemoveObj` (**840**).
