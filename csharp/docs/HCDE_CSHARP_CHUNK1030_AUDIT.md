# Chunk 1030 — `ParseMapInfo` late top-level keywords, `G_ClearMapinfo`, and `HCDE_ParseCMPGNINF` scanner helpers from `g_mapinfo.cpp` (**2750–2899**). Follows chunk **1029** (**2600–2749**). Hull ends inside `mapname` token validation (**2896–2899**); apply/skip tail (**2920+**) deferred to next band.

## Scope

`ParseMapInfo` late top-level keywords, `G_ClearMapinfo`, and `HCDE_ParseCMPGNINF` scanner helpers from `g_mapinfo.cpp` (**2750–2899**). Follows chunk **1029** (**2600–2749**). Hull ends inside `mapname` token validation (**2896–2899**); apply/skip tail (**2920+**) deferred to next band.

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseMapInfo tail / G_ClearMapinfo / HCDE_ParseCMPGNINF head** 2750–2899 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMapInfo1030Slice.cs`
Managed LOC (tally): 128.
Conversion commit: `d7a7111c6df63816e3764ab255f19c9bdb8304b8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `damagetype` / `spawnnums` / `conversationids` new-format gates (**2752–2787**) | yes |
| `automap` / `automap_overlay` gate + overlay flag (**2788–2798**) | yes |
| Unknown top-level keyword error (**2800–2802**) | yes |
| `G_ClearMapinfo` reset fields (**2815–2826**) | yes (policy snapshot) |
| Indexed CMPGNINF property bracket depth (**2835–2845**) | yes |
| `HCDE_SkipCMPGNINFValue` comma continuation (**2860–2865**) | yes (predicate) |
| `mapname` token kind validation (**2894–2899**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2600–2749** | chunk **1029** |
| **2750–2751** | `doomednums` old-format error tail (seam with **1029**) |
| **2805–2813** | closing braces / forward decl host |
| **2874–2877**, **2879–2881** | lump find / scanner host |
| **2900+** | outside hull (**2899** end) |
| **2920+** | CMPGNINF apply/skip block (next band) |
| **3067–3114** | off-tally `VmMapInfoParseMapInfoLoop` |
| Retired chunk **303** | no tally re-credit; `VmCmpgninfParser` seam test only |

| Managed (`VmMapInfoParseMapInfo1030Slice.cs`) | Native (`g_mapinfo.cpp`) |
|--------------------------------------------------|---------------------------|
| `TryParseLateTopLevelKeyword` | **2752–2802** |
| `CreateClearedMapinfoState` | **2815–2826** |
| `AdvanceIndexedPropertyBracketDepth` | **2835–2845** |
| `ShouldContinueSkippingCmpgninfValue` | **2860–2865** |
| `TryValidateCmpgninfMapNameToken` | **2896–2899** |
| Band hull (exclusive slice) | **2750–2899** |

### Chunk **1030** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1029** |


**Post-review ratio:** 128/150 managed/native ≈ 85% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMapInfo1030SliceTests.TryParseLateTopLevelKeyword_RejectsDamageTypeOnOldFormat`
- `VmMapInfoParseMapInfo1030SliceTests.TryParseLateTopLevelKeyword_AcceptsSpawnNumsOnNewFormat`
- `VmMapInfoParseMapInfo1030SliceTests.TryParseLateTopLevelKeyword_DetectsAutomapOverlay`
- `VmMapInfoParseMapInfo1030SliceTests.FormatUnknownTopLevelKeyword_MatchesNativeShape`
- `VmMapInfoParseMapInfo1030SliceTests.CreateClearedMapinfoState_MatchesNativeDefaults`
- `VmMapInfoParseMapInfo1030SliceTests.AdvanceIndexedPropertyBracketDepth_TracksNestedBrackets`
- `VmMapInfoParseMapInfo1030SliceTests.TryValidateCmpgninfMapNameToken_RejectsInvalidToken`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1030 --to 1030` (0 hard / 0 soft).

## Seams

`ParseDamageDefinition`, `ParseSpawnNums`, `ParseConversationIDs`, `ParseAMColors`, `DeinitIntermissions`, `HCDE_ClearEMapInfoCompat`, `fileSystem.FindLump`, and `FScanner` I/O remain native.

## Credit rationale

UNCredited contiguous **`g_mapinfo.cpp`** band after **1029** with zero conversion/recognition overlap and no Vm* line-range collision on **2750–2899** (`VmMapInfoParseMapInfoLoop` **3067–3114** outside hull).
