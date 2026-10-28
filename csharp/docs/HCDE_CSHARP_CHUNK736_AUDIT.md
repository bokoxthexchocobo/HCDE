# Chunk 736 — Whole `SerializeModelID` through `ModelAnimFrame` archive (`p_mobj.cpp` 1749–1918).

## Scope

Whole `SerializeModelID` through `ModelAnimFrame` archive (`p_mobj.cpp` 1749–1918).

## Native ranges

- `src/playsim/p_mobj.cpp` **SerializeModelID** 1749–1918 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmPMobjSerializeModel.cs`
Managed LOC (tally): 159.
Conversion commit: `9f4ee8dd94cd3928f974e029a7fba2f4476df9ac`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SerializeModelId` | `SerializeModelID` 1749–1772 |
| `SerializeBoneOverride` / `SerializeBoneOverrideComponent` | `Serialize(BoneOverride)` 1788–1796 |
| `SerializeTrs` | `Serialize(TRS)` 1798–1806 |
| `SerializeModelOverride` | `Serialize(ModelOverride)` 1808–1815 |
| `SerializeAnimModelOverride` | `Serialize(AnimModelOverride)` 1817–1830 |
| `SerializeModelAnim` | `Serialize(ModelAnim)` 1832–1845 |
| `ReadModelAnimFrameType` / `WriteModelAnimFrameType` / `LegacyFirstFrameKeyPresent` | `Serialize(ModelAnimFrame)` 1847–1918 |
| `ShouldClearLegacyInterp` | legacy read branch 1852–1855 |
| `SerializeBoneOverrideComponent` fields | `SerializeBoneOverrideComponent` 1775–1785 |
| `ModelOverrideFields.SurfaceSkinIds` | `Serialize(ModelOverride)` surface skins 1812 |
| `AnimModelOverrideFields` null object path | `Serialize(AnimModelOverride)` read guard 1820–1823 |


**Post-review ratio:** 159/170 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSerializeModelTests.SerializeModelId_ReadMissingKey_ReturnsNegativeOne`
- `VmPMobjSerializeModelTests.SerializeModelId_WritePositiveId_EmitsPath`
- `VmPMobjSerializeModelTests.ReadModelAnimFrameType_PrecalcMapsToNull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 736 --to 736` (0 hard / 0 soft).

## Seams

`FSerializer`, `Models[]`, and `FindModel` IO are injected.

## Credit rationale

Model archive band: model id path round-trip plus anim frame type dispatch.
