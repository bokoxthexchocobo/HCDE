# Chunk 1012 — `CCMD(map)`, `recordmap`, and `open` from `g_level.cpp` (**301–450**); follows chunk **1011** (**151–300**).

## Scope

`CCMD(map)`, `recordmap`, and `open` from `g_level.cpp` (**301–450**); follows chunk **1011** (**151–300**).

## Native ranges

- `src/g_level.cpp` **CCMD(map)** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelMapCommands1012Slice.cs`
Managed LOC (tally): 86.
Conversion commit: `1a1277deecfd8163e871feb408b654ecec42aaf1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `CCMD(map)` net/usage/map name (**305–348**) | yes |
| Coop/dm flags (**326–335**, **376–385**, **426–435**) | yes |
| `recordmap` (**356–401**) | yes |
| `open` (**409–443**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **300–304** | comment seam with **1011** |
| **318–343** | try/catch + `Printf` host |
| **336–337** | `G_DeferedInitNew` host call |
| **387–389** | demo name assignment host |
| **452+** | `G_NewInit` (next band) |

| Managed (`VmGLevelMapCommands1012Slice.cs`) | Native (`g_level.cpp`) |
|---------------------------------------------|----------------------|
| SP-only reject / argc gates | **307–347**, **358–400**, **411–442** |
| `ResolveMapNameArgument` | **316–316**, **366–366** |
| Coop/dm flag resolution | **326–335** etc. |
| Record/open deferred setup | **386–437** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1012** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1011** |


**Post-review ratio:** 86/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelMapCommands1012SliceTests.MapCommand`
- `VmGLevelMapCommands1012SliceTests.RecordMapCommand`
- `VmGLevelMapCommands1012SliceTests.OpenCommand`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1012 --to 1012` (0 hard / 0 soft).

## Seams

`Printf`, `P_CheckMapData`, `G_DeferedInitNew`, recoverable-error print, and `G_NewInit` (**452+**) remain injected.

## Credit rationale

UNCredited **301–450** CLI band; disjoint from chunks **1010–1011**.
