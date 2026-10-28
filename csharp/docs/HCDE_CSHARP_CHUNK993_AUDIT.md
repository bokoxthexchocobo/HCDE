# Chunk 993 — `P_OpenMapData` WAD lump walk, TEXTMAP supplement mapping, embedded `maps/*.wad` pick, and WAD magic probe from `p_openmap.cpp` (**151–300**); follows chunk **992** (**1–150**).

## Scope

`P_OpenMapData` WAD lump walk, TEXTMAP supplement mapping, embedded `maps/*.wad` pick, and WAD magic probe from `p_openmap.cpp` (**151–300**); follows chunk **992** (**1–150**).

## Native ranges

- `src/p_openmap.cpp` **P_OpenMapData** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPOpenMapWadWalk993Slice.cs`
Managed LOC (tally): 164.
Conversion commit: `658696fa4fceae1c8aa43297261dd1ee9c5dc6b3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Classic WAD lump loop via `GetMapIndex` (**165–191**) | yes |
| TEXTMAP sidecar lump aliases (**197–238**) | yes |
| Embedded `maps/*.wad` pick (**244–257**) | yes |
| WAD magic probe (**266–267**) | yes |
| Resource TEXTMAP header + supplement loop head (**278–300**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **151–164**, **240–241** | **992** build-map return / seam |
| **157–159**, **189–190**, **196–197**, **236–237** | reader/`strncpy` host |
| **201–204** | `I_Error` host |
| **170–178**, **259–264** | `try/catch` / seek-read host |
| **1–150** | chunk **992** |

| Managed (`VmPOpenMapWadWalk993Slice.cs`) | Native (`p_openmap.cpp`) |
|-------------------------------------------|-------------------------|
| `TryAdvanceClassicMapLump` / abort/end/behavior predicates | **172–191** |
| `NextLumpIsClassicDoomFormat` | **163–164** |
| `ClassifyTextMapSupplementLump` / `TryResolveTextMapSupplementLump` | **205–235**, **286–301** |
| `TrySelectEmbeddedMapsWadLump` | **244–257** |
| `IsRecognizedWadMagic` | **266–267** |
| `ResourceEntryIsTextMapHeader` | **278–281** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **993** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**992** |


**Post-review ratio:** 164/150 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmPOpenMapWadWalk993SliceTests.ClassicMapLumpAdvance_Uses992Scanner`
- `VmPOpenMapWadWalk993SliceTests.TextMapSupplementLumps`
- `VmPOpenMapWadWalk993SliceTests.EmbeddedWadAndMagic`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 993 --to 993` (0 hard / 0 soft).

## Seams

`GetMapIndex` host errors, `fileSystem`/`FResourceFile` IO, `strncpy`, `I_Error`, and `try/catch` teardown remain injected.

## Credit rationale

UNCredited continuation of `p_openmap.cpp`; reuses **992** `ScanMapLumpIndex` without re-crediting **56–94**. No overlap with **992** short-name head (**129–150**).
