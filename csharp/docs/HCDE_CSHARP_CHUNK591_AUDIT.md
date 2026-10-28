# Chunk 591 — FBehavior::SerializeVars, SerializeVarSet, ParseLocalArrayChunk, ctor, Init header gate (2171–2327)

## Scope

FBehavior::SerializeVars, SerializeVarSet, ParseLocalArrayChunk, ctor, Init header gate (2171–2327)

## Native ranges

- `src/playsim/p_acs.cpp` **FBehavior::SerializeVars** 2171–2182 (12 lines)
- `src/playsim/p_acs.cpp` **FBehavior::SerializeVarSet** 2184–2238 (55 lines)
- `src/playsim/p_acs.cpp` **ParseLocalArrayChunk** 2240–2258 (19 lines)
- `src/playsim/p_acs.cpp` **FBehavior::FBehavior** 2260–2278 (19 lines)
- `src/playsim/p_acs.cpp` **FBehavior::Init** 2281–2327 (47 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmAcsBehaviorVarSerialize.cs`
Managed LOC (tally): 89.
Conversion commit: `d7361f739193b304b089e665ab1d1775fcee2a74`.

## Padding review (2026-10-21)

Whole-function ports only; measured after faithful port (no LOC target).

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `SerializeVarsNative` | `FBehavior::SerializeVars` 2171–2182 |
| `SerializeVarSetWriteBounds` / `SerializeVarSetReadNative` | `FBehavior::SerializeVarSet` 2184–2238 |
| `ParseLocalArrayChunkNative` | `ParseLocalArrayChunk` 2240–2258 |
| `ResetBehaviorCtorNative` | `FBehavior::FBehavior` 2260–2278 |
| `InitRejectsTinyLumpNative` / `InitReadsAcsMagicNative` | `FBehavior::Init` 2281–2327 |

**Post-review ratio:** 89/152 managed/native ≈ 59% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsBehaviorVarSerializeTests.SerializeVarsNative_WalksMapAndArrays`
- `VmAcsBehaviorVarSerializeTests.SerializeVarSetWriteBounds_AllZero_ReturnsEmpty`
- `VmAcsBehaviorVarSerializeTests.SerializeVarSetWriteBounds_TrimsLeadingTrailingZeros`
- `VmAcsBehaviorVarSerializeTests.SerializeVarSetReadNative_CapsCountToMax`
- `VmAcsBehaviorVarSerializeTests.ParseLocalArrayChunkNative_AccumulatesSizes`
- `VmAcsBehaviorVarSerializeTests.ResetBehaviorCtorNative_ZerosCoreFields`
- `VmAcsBehaviorVarSerializeTests.InitReadsAcsMagicNative_RequiresAcsPrefix`
- `VmAcsBehaviorVarSerializeTests.InitRejectsTinyLumpNative_Below32`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 591 --to 591` (0 hard / 0 soft).

## Seams

`FSerializer`, `fileSystem`, and `new uint8_t[]` allocation are not hosted.

## Credit rationale

Whole-function ports for ACS variable serialization and behavior lump header validation.
