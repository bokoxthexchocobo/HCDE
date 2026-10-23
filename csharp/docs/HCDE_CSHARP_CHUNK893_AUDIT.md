# Chunk 893 — `AlignPlane`, `SetSlopes`, and `CopySlopes` (`slopes.cpp` 439–588).

## Scope

`AlignPlane`, `SetSlopes`, and `CopySlopes` (`slopes.cpp` 439–588).

## Native ranges

- `src/maploader/slopes.cpp` **MapLoader::AlignPlane** 439–501 (63 lines)
- `src/maploader/slopes.cpp` **slopes.cpp SetSlopes section gap** 502–508 (7 lines)
- `src/maploader/slopes.cpp` **MapLoader::SetSlopes** 509–539 (31 lines)
- `src/maploader/slopes.cpp` **slopes.cpp CopySlopes section gap** 540–546 (7 lines)
- `src/maploader/slopes.cpp` **MapLoader::CopySlopes** 547–588 (42 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapLoaderSlopesAlignCopy.cs`
Managed LOC (tally): 103.
Conversion commit: `8201958d3d35c55e4f472ff60df73f0c1f3d939c`.

## Padding review (2026-10-21)

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `MapLoader::AlignPlane` **439–501** | **yes** |
| `MapLoader::SetSlopes` **509–539** | **yes** |
| `MapLoader::CopySlopes` **547–588** | **yes** |
| `slopes.cpp` inter-function separators **502–508**, **540–546** | **no** (hull credit only) |

| Managed | Native (`slopes.cpp`) |
|---------|----------------------|
| `AlignPlane` | **439–501** |
| `SetSlopes` | **509–539** |
| `CopySlopes` | **547–588** |
| (separators) | **502–508**, **540–546** |

### Padding map (hull **439–588**)

| Native lines | Reason |
|--------------|--------|
| **502–508** | `P_SetSlopes` banner + blank between `AlignPlane` and `SetSlopes` |
| **540–546** | `P_CopySlopes` banner + blank between `SetSlopes` and `CopySlopes` |

### Hull-gap table (**150 − 150 = 0**)

Every hull line **439–588** appears in `nativeRanges` (three whole functions plus two separator spans). No partial-function picking inside credited bodies.

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **150** |
| Net behavior-ported | **136** |
| Gross CLI hull (**439–588**) | **150** |


**Post-review ratio:** 103/150 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmMapLoaderSlopesTests.AlignPlane_NoOpWhenLineHasNoBackSector`
- `VmMapLoaderSlopesTests.AlignPlane_TiltsFrontFloorTowardOppositeSectorHeight`
- `VmMapLoaderSlopesTests.SetSlopes_ClearsPlaneAlignSpecialAndAlignsFront`
- `VmMapLoaderSlopesTests.CopySlopes_CopiesFloorByTagAndClearsSpecial`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 893 --to 893` (0 hard / 0 soft).

## Seams

**Representation notes:** `SlopeLine.Special` and `Plane_Align` / `Plane_Copy` constants live on the shared `VmMapLoaderSlopes` partial (chunk 892 types); this row credits only `VmMapLoaderSlopesAlignCopy.cs` for the three `MapLoader` methods. `Level->lines` iteration uses `SlopeLevel.Lines`.

## Credit rationale

Faithful line-special align and tag-based slope copy after thing-driven slope setup (chunk 892).
