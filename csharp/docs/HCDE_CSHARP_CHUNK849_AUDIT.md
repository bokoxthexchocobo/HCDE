# Chunk 849 — `FParser::SF_SpawnShot2` … `SaveCurrentScript` from `fragglescript/t_func.cpp` (3700–3849).

## Scope

`FParser::SF_SpawnShot2` … `SaveCurrentScript` from `fragglescript/t_func.cpp` (3700–3849).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SpawnShot2** 3707–3743 (37 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_KillInSector** 3753–3766 (14 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetLineTrigger** 3776–3799 (24 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::RunLineSpecial** 3808–3821 (14 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SaveCurrentScript** 3830–3842 (13 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF gzd separators** 3700–3706 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF gzd separators** 3744–3752 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF gzd separators** 3767–3775 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF gzd separators** 3800–3807 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF gzd separators** 3822–3829 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF gzd separators** 3843–3849 (7 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmFScriptParserSfGzd.cs`
Managed LOC (tally): 86.
Conversion commit: `b8935fb0b1aff3f6fb589683ffaf77ec13366795`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_SpawnShot2` | yes |
| `SF_KillInSector` | yes |
| `SF_SetLineTrigger` | yes |
| `RunLineSpecial` | yes |
| `SaveCurrentScript` | yes |

| Managed | Native (`t_func.cpp` 3700–3849) |
|---------|-------------------------|
| `SfSpawnShot2` | **3707–3743** |
| `SfKillInSector` | **3753–3766** |
| `SfSetLineTrigger` | **3776–3799** |
| `SfRunLineSpecial` | **3808–3821** |
| `SaveCurrentScript` | **3830–3842** |
| (separators) | **3700–3706**, **3744–3752**, **3767–3775**, **3800–3807**, **3822–3829**, **3843–3849** |

**Post-review ratio:** 86/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfGzdTests.SfSpawnShot2_SpawnsProjectile`
- `VmFScriptParserSfGzdTests.SfKillInSector_MassacresMonstersInTag`
- `VmFScriptParserSfGzdTests.SfSetLineTrigger_SetsSpecialOnId`
- `VmFScriptParserSfGzdTests.SfRunLineSpecial_ExecutesDoor`
- `VmFScriptParserSfGzdTests.SaveCurrentScript_EnqueuesRunner`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 849 --to 849` (0 hard / 0 soft).

## Seams

Uses `SpawnProjectile`, `TagManager` sector tags, line id lookup, `LineSpecials.TryActivate`, and `VmFScriptThinker.AddRunningScript`.

## Credit rationale

GZDoom spawn/kill/line band after **847** `setcolor`; script wait tail in **850**.
