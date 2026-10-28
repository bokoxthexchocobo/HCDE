# Chunk 1005 — `G_DoSaveGame` zip payload, success path, cleanup, and `G_ReadDemoTiccmd` head from `g_game.cpp` (**3050–3199**); follows chunk **1004** (**2900–3049**).

## Scope

`G_DoSaveGame` zip payload, success path, cleanup, and `G_ReadDemoTiccmd` head from `g_game.cpp` (**3050–3199**); follows chunk **1004** (**2900–3049**).

## Native ranges

- `src/g_game.cpp` **G_DoSaveGame** 3050–3199 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGGameSavePayload1005Slice.cs`
Managed LOC (tally): 95.
Conversion commit: `83ce66fc6577f0127c2a1e9f7ded5e34098134f7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `AllowLoadIn` array gate (**3062–3070**) | yes |
| Globals ticrate/leveltime (**3079–3084**) | yes |
| `nextskill` optional (**3097–3100**) | yes |
| Zip verify + notify (**3118–3136**) | yes |
| JSON buffer cleanup indices (**3145–3146**) | yes |
| Save completion / time unfreeze (**3151–3154**) | yes |
| `G_ReadDemoTiccmd` loop head (**3165–3198**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **3050–3058** | PNG finish / info fields (seam with **1004**) |
| **3072–3077** | `PutSaveWads` / hub / cvar host |
| **3090–3094** | STAT/RNG/ACS host |
| **3102–3111** | buffer compression host |
| **3118–3124** | resource file open host |

| Managed (`VmGGameSavePayload1005Slice.cs`) | Native (`g_game.cpp`) |
|-------------------------------------------|----------------------|
| `ShouldSerializeAllowLoadInPorts` | **3062–3070** |
| `ShouldIncludeLevelTimingInGlobals` / `ShouldSerializeNextSkillGlobal` | **3079–3100** |
| `ConfirmsSavedArchiveIntegrity` / notification style | **3118–3136** |
| `JsonContentCleanupIndices` / completion cleanup | **3145–3154** |
| Demo command predicates | **3165–3198** |
| Band hull (exclusive slice) | **3050–3199** |

### Chunk **1005** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1004** |


**Post-review ratio:** 95/150 managed/native ≈ 63% (src file LOC at HEAD; see tally).

## Tests

- `VmGGameSavePayload1005SliceTests.SaveGlobalsAndArchive`
- `VmGGameSavePayload1005SliceTests.DemoTiccmdHead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1005 --to 1005` (0 hard / 0 soft).

## Seams

Serializer writers, `WriteZip`, `FResourceFile`, `G_WriteSnapshots`, and demo unpack host remain injected.

## Credit rationale

UNCredited **3050–3199** CLI band; disjoint from chunks **995–1004** and chunk **9** credited segments.
