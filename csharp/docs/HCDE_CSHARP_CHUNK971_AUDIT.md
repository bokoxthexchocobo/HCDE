# Chunk 971 — P-code enum tail **369–384**, `PCODE_COMMAND_COUNT`, ACS script constant enums, HUD/LineAttack macros, and `ACSToDouble`/`ACSToFloat` head from `p_acs.cpp` (**451–600**); follows chunk **970** (**≤450**).

## Scope

P-code enum tail **369–384**, `PCODE_COMMAND_COUNT`, ACS script constant enums, HUD/LineAttack macros, and `ACSToDouble`/`ACSToFloat` head from `p_acs.cpp` (**451–600**); follows chunk **970** (**≤450**).

## Native ranges

- `src/playsim/p_acs.cpp` **PCODE_COMMAND_COUNT** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsHeadConstants971Slice.cs`
Managed LOC (tally): 130.
Conversion commit: `22305ed8861874b23f138a1c7b285b13252ff74a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| PCD tail **369–384** + `PCODE_COMMAND_COUNT` (**451–468**) | yes |
| LINE/SIDE/TEXTURE/GAME/CLASS/SKILL/BLOCK enums (**472–509**) | yes |
| LEVELINFO / PLAYERINFO enums (**510–534**) | yes |
| `STACK_SIZE`, HUDMSG, FHF, SDF, ARMORINFO, PICKAF (**539–589**) | yes |
| `ACSToDouble` / `ACSToFloat` (**596–604** partial in band) | yes (scale **65536**) |
| `FRandom` decls (**536–537**) | layout only |

| Managed (`VmPAcsHeadConstants971Slice.cs`) | Native (`p_acs.cpp`) |
|-------------------------------------------|---------------------|
| `Pcd*` tail / `PcodeCommandCount` / `ValidPcodeOpcode` | **451–468** |
| Script constant ints | **472–534** |
| Macro mirrors | **539–589** |
| `AcsToDouble` / `AcsToFloat` / `DoubleToAcs` | **596–608** policy |
| Band hull (exclusive slice) | **451–600** |

### Chunk **971** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**970** |


**Post-review ratio:** 130/150 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsHeadConstants971SliceTests.PcodeCommandCount_MatchesNativeEnum`
- `VmPAcsHeadConstants971SliceTests.EnumTail_IncludesTranslationRange5`
- `VmPAcsHeadConstants971SliceTests.AcsFixedPointConversions`
- `VmPAcsHeadConstants971SliceTests.HudmsgLayerMask_MatchesNativeShift`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 971 --to 971` (0 hard / 0 soft).

## Seams

`FRandom pr_acs` / `CallReturn` and remainder of fixed-point inlines after **600** remain host-only; chunk **789** string pool (**945+**) is disjoint.

## Credit rationale

UNCredited band after **970** with zero conversion overlap; **789** islands at **828–901** and **945–1291** do not intersect **451–600**.
