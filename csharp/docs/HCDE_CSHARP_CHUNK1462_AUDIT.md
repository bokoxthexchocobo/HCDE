# Chunk 1462 — MapLoader::SpawnSpecials sector scan and line specials in specials.cpp (624-852).

## Scope

MapLoader::SpawnSpecials sector scan and line specials in specials.cpp (624-852).

## Native ranges

- `src/maploader/specials.cpp` **MapLoader::SpawnSpecials** 624–852 (229 lines)

**Credited native LOC:** 229 (tally `nativeLoc` 229).

## Managed

`VmSpecials1462Slice.cs`
Managed LOC (tally): 311.
Conversion commit: `7ef38951cf91fac1f89ebc6dbcb755f2232b83ec`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| SpawnSpecials (**624–852**) | yes |
| Band hull (**624–852**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Banner | lines 615-623 stay uncredited |
| SpawnFriction | starts at line 860 |

Sectors with a special are handed to InitSectorSpecial. Transfer_Heights copies the control sector onto each tagged sector and sets HasHeightSecs when the ignore bit is clear. A non-repeatable Generic_Lift whose target is not 1 leaves the line. Static_Init gravity, damage, links, and sky transfer run here. Open scripts start after the line scan.

| Managed (`VmSpecials1462Slice.cs`) | Native (`specials.cpp`) |
|-------------------------------------|------------------------|
| Spawn | **624–852** |
| Band hull (exclusive slice) | **624–852** |

### Chunk **1462** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **229** |
| **Net headline** | **+229** vs the prior chunk |


**Post-review ratio:** 311/229 managed/native ≈ 136% (src file LOC at HEAD; see tally).

## Tests

- `VmSpecials1462SliceTests.SetupOrderSkipsPlainSectorsAndOpensScripts`
- `VmSpecials1462SliceTests.HeightsPortalsLiftsAndStaticInit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1462 --to 1462` (0 hard / 0 soft).

## Seams

MapLoader::SpawnSpecials from its opening brace through the SCRIPT_Open call. The banner at 615-623 stays uncredited. SpawnFriction starts at 860.

## Credit rationale

UNCredited specials.cpp 624-852 (+229). One contiguous function. The span does not cross credited or recognition lines.
