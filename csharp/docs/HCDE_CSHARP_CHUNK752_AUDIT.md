# Chunk 752 — Whole model archive serialize helpers (`p_mobj.cpp` 1749–1918); excludes `DActorModelData` / `DViewPosition` class methods. Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Scope

Whole model archive serialize helpers (`p_mobj.cpp` 1749–1918); excludes `DActorModelData` / `DViewPosition` class methods. Retired (2026-10-23): out-of-body credit removed; honest **149** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `5d622d28a22f4878b4ff3bcd4e182049ced18523`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-22):** Credits eight free functions **1749–1918** only. Excludes `DActorModelData::Serialize` **1920–1934**, `DActorModelData::OnDestroy` **1936–1943**, and `DViewPosition::Serialize` **1945–1950** (require `Super::Serialize` and collection types).

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SerializeModelId` | `SerializeModelID` 1749–1772 |
| `SerializeBoneOverrideComponent` | `SerializeBoneOverrideComponent` (template) 1774–1786 |
| `SerializeBoneOverride` | `Serialize(BoneOverride)` 1788–1796 |
| `SerializeTrs` / `IModelArchiveFieldSink.ArcVector3` | `Serialize(TRS)` 1798–1806 |
| `SerializeModelOverride` | `Serialize(ModelOverride)` 1808–1815 |
| `SerializeAnimModelOverride` | `Serialize(AnimModelOverride)` 1817–1830 |
| `SerializeModelAnim` | `Serialize(ModelAnim)` 1832–1845 |
| `ShouldClearLegacyInterp` | legacy read branch 1852–1855 |
| `ReadModelAnimFrameType` | read `type` dispatch 1858–1883 |
| `SerializeModelAnimFrame` | `Serialize(ModelAnimFrame)` 1847–1918 |
| `WriteModelAnimFrameTypeToken` | write `type` branch 1888–1914 |
| `ModelAnimFrameInterpFields` | `ModelAnimFrameInterp` read/write 1866–1870, 1897–1899 |

### Out-of-body uncredit (752)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **163** |
| **Out-of-body removed** | **14** |
| **Honest** | **149** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 752 --to 752` (0 hard / 0 soft).

## Seams

`FSerializer`, `Models[]`, `FindModel`, and nested `arc` IO are injected via `IModelArchiveFieldSink` in uncredited `ModelArchiveFieldSink.cs` / `ModelArchiveContext`.

## Credit rationale

Re-ports retired chunk736 band with faithful whole-function serialize helpers only (1920+ deferred until full class archive ports).
