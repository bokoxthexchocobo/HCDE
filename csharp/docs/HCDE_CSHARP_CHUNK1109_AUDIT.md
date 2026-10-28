# Chunk 1109 — TRS/model override serializers, `ModelAnimFrame` type tags, `DActorModelData::OnDestroy`, and view-position keys from `p_mobj.cpp` (**1801–1950**); follows **1108** visibility band.

## Scope

TRS/model override serializers, `ModelAnimFrame` type tags, `DActorModelData::OnDestroy`, and view-position keys from `p_mobj.cpp` (**1801–1950**); follows **1108** visibility band.

## Native ranges

- `src/playsim/p_mobj.cpp` **Model serialize and view position** 1801–1950 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjModelSerialize1109Slice.cs`
Managed LOC (tally): 112.
Conversion commit: `b6bfb42ac164b13cc1f3ae74a6a2a894d282d15a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `AnimModelOverride` missing object (**1820–1823**) | yes |
| `ModelAnimFrame` read tags (**1850–1883**) | yes |
| Write `precalcIQM` as nullptr (**1901–1913**) | yes |
| `DActorModelData::OnDestroy` resets (**1938–1943**) | yes |
| View position keys (**1947–1949**) | yes (key list) |
| TRS field keys (**1801–1804**) | yes (key list) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1801–1814**, **1834–1843** | host `arc()` field writes |
| **1920–1934** | host serializer chain |
| **1958+** | `P_ExplodeMissile` outside hull |

| Managed (`VmPMobjModelSerialize1109Slice.cs`) | Native (`p_mobj.cpp`) |
|----------------------------------------------|---------------------|
| Frame tag + destroy + key lists | **1801–1950** |
| Band hull (exclusive slice) | **1801–1950** |

### Chunk **1109** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1108** |


**Post-review ratio:** 112/150 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjModelSerialize1109SliceTests.ApplyAnimModelOverrideRead_SetsMissingId`
- `VmPMobjModelSerialize1109SliceTests.ApplyModelAnimFrameRead_ClearsPrecalculatedIqm`
- `VmPMobjModelSerialize1109SliceTests.FrameKindWriteTag_ArchivesPrecalcAsNull`
- `VmPMobjModelSerialize1109SliceTests.RunModelDataDestroy_ZerosCollections`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1109 --to 1109` (0 hard / 0 soft).

## Seams

`FSerializer` I/O and `SerializeModelID` remain host-injected; **1951+** continues `P_ExplodeMissile`.

## Credit rationale

UNCredited **`p_mobj.cpp`** **1801–1950** (+150). Skips **1501–1685** (**734** Grind) and `s_sound` **601–1050** / **1801–1874**.
