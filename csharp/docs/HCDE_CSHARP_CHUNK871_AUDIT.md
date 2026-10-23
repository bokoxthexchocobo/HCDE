# Chunk 871 — `FParser::SF_LineTrigger` … `SF_SetLineTexture` from `fragglescript/t_func.cpp` (2040–2239); `runcommand` / `changemusic` not registered.

## Scope

`FParser::SF_LineTrigger` … `SF_SetLineTexture` from `fragglescript/t_func.cpp` (2040–2239); `runcommand` / `changemusic` not registered.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_LineTrigger** 2058–2070 (13 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetLineBlocking** 2115–2134 (20 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetLineMonsterBlocking** 2142–2156 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetLineTexture** 2172–2239 (68 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF line separators** 2040–2045 (6 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF line separators** 2071–2076 (6 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF line separators** 2106–2114 (9 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF line separators** 2135–2141 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF line separators** 2157–2171 (15 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmFScriptParserSfLineOps.cs`, `VmFScriptInitFunctionsReg871.cs`
Managed LOC (tally): 124.
Conversion commit: `006aa1a919e23fe9b7b03e09a000c3210fd89629`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported | Registered |
|---------------|-----------------|------------|
| `SF_LineTrigger` | yes | `linetrigger` |
| `SF_SetLineBlocking` | yes | `setlineblocking` |
| `SF_SetLineMonsterBlocking` | yes | `setlinemonsterblocking` |
| `SF_SetLineTexture` | yes | `setlinetexture` |

**Not in this chunk (zero credit, not registered)**

| Native | Policy |
|--------|--------|
| `SF_RunCommand` (**2047–2050**) | Unregistered |
| `FS_ChangeMusic` / `SF_ChangeMusic` (**2077–2105**) | Unregistered |

| Managed | Native |
|---------|--------|
| `SfLineTrigger` | **2058–2070** |
| `SfSetLineBlocking` | **2115–2134** |
| `SfSetLineMonsterBlocking` | **2142–2156** |
| `SfSetLineTexture` | **2172–2239** |
| `VmFScriptInitFunctionsReg871.Register` | `NewFunction` rows for ported names only |
| (separators) | **2040–2045**, **2071–2076**, **2106–2114**, **2135–2141**, **2157–2171** |
| (unregistered band) | **2047–2050**, **2077–2105** |

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **159** |

**Post-review ratio:** 124/159 managed/native ≈ 78% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfLineOpsTests.SfLineTrigger_CallsFloorSpecial`
- `VmFScriptParserSfLineOpsTests.SfSetLineBlocking_SetsBlockingFlag`
- `VmFScriptParserSfLineOpsTests.SfSetLineMonsterBlocking_SetsMonsterBlock`
- `VmFScriptParserSfLineOpsTests.SfSetLineTexture_EternityLayout_UpdatesMid`
- `VmFScriptParserSfLineOpsTests.SfSetLineTexture_LegacyLayout_UpdatesTopAndBottom`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 871 --to 871` (0 hard / 0 soft).

## Seams

Line trigger uses `ActorSpecialActions.CallSpecial`; blocking/texture mutate `LevelLine` / `LevelSide` via `AcsLineActivation.LinesFromId`. No placeholder registrations.

## Credit rationale

Line-helper band after **870**; `SF_RunCommand` / `SF_ChangeMusic` stay off `Reg871` until real `FS_EmulateCmd` / `S_ChangeMusic` ports land.
