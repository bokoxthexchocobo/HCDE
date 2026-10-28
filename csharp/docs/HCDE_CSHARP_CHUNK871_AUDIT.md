# Chunk 871 — `FParser::SF_LineTrigger` … `SF_SetLineTexture` from `fragglescript/t_func.cpp` (2040–2239); `runcommand` / `changemusic` not registered. Retired (2026-10-23): out-of-body credit removed; honest **128** < **150**.

## Scope

`FParser::SF_LineTrigger` … `SF_SetLineTexture` from `fragglescript/t_func.cpp` (2040–2239); `runcommand` / `changemusic` not registered. Retired (2026-10-23): out-of-body credit removed; honest **128** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `cf0888f893d19088192469f7e0515c308e555035`.

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

### Out-of-body uncredit (871)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **159** |
| **Out-of-body removed** | **31** |
| **Honest** | **128** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 871 --to 871` (0 hard / 0 soft).

## Seams

Line trigger uses `ActorSpecialActions.CallSpecial`; blocking/texture mutate `LevelLine` / `LevelSide` via `AcsLineActivation.LinesFromId`. No placeholder registrations.

## Credit rationale

Line-helper band after **870**; `SF_RunCommand` / `SF_ChangeMusic` stay off `Reg871` until real `FS_EmulateCmd` / `S_ChangeMusic` ports land.
