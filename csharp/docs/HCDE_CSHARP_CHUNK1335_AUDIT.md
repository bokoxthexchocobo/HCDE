# Chunk 1335 — DCeiling construct, FLevelLocals::CreateCeiling, the CreateCeiling action wrapper, and EV_DoCeiling from a_ceiling.cpp (216-551), through the in-stasis comment.

## Scope

DCeiling construct, FLevelLocals::CreateCeiling, the CreateCeiling action wrapper, and EV_DoCeiling from a_ceiling.cpp (216-551), through the in-stasis comment.

## Native ranges

- `src/playsim/mapthinkers/a_ceiling.cpp` **CreateCeiling** 216–551 (336 lines)

**Credited native LOC:** 336 (tally `nativeLoc` 336).

## Managed

`VmACeilingConstruct1335Slice.cs`
Managed LOC (tally): 543.
Conversion commit: `e3e17926b99fa5c089b27288fa35d4424b66d8ff`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Tick closing brace (**216–216**) | yes |
| One-argument construct (**223–226**) | yes |
| Four-argument construct (**228–242**) | yes |
| CreateCeiling (**250–479**) | yes |
| CreateCeiling action wrapper (**481–495**) | yes |
| EV_DoCeiling (**506–542**) | yes |
| In-stasis comment (**545–551**) | yes |
| Band hull (**216–551**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| DCeiling::Tick | chunk 797, lines 125-215 |
| ActivateInStasisCeiling body | chunk 797, lines 552-568 |
| FindHighestFloorPoint and the other surrounding-height finders | callees; they may replace the spot vertex |
| FindModelFloorSector and FindModelCeilingSector | callees; a null model skips the texture change |
| ActivateInStasisCeiling | callee of EV_DoCeiling; the manual path calls it and then returns only CreateCeiling |

ECeiling values follow a_ceiling.h. Crush-and-raise and crush-raise-and-stay fall through into lower-and-crush after storing the current ceiling distance. A moving or blocked ceiling returns false before construct. The four-argument construct stores silent with bit 2 cleared. Instant types replace speed with height before the distance check. When speed is at least the move distance, interpolation stops and silent bit 2 stores 2. Numeric model change uses the floor model only for raise-to-floor and lower-to-floor. Case 2 of that change copies the moving sector. The trigger change copies the front sector. A null model does not fall through to the line. The action wrapper passes tag 0. A manual tag of 0 rewrites the tag with the back sector number or 0x1000000, calls the in-stasis callee, and returns the create result. Only crush-and-raise restarts in-stasis ceilings before the tag iterator. The iterator stops on a negative sector number.

| Managed (`VmACeilingConstruct1335Slice.cs`) | Native (`a_ceiling.cpp`) |
|------------------------------------------------|--------------------------|
| Construct, CreateCeiling, the action wrapper, and EV_DoCeiling | **216–551** |
| Band hull (exclusive slice) | **216–551** |

### Chunk **1335** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **336** |
| **Net headline** | **+336** vs post-**1334** |


**Post-review ratio:** 543/336 managed/native ≈ 162% (src file LOC at HEAD; see tally).

## Tests

- `VmACeilingConstruct1335SliceTests.ConstructFieldsTypesAndTextureChange`
- `VmACeilingConstruct1335SliceTests.EvManualCrushIteratorAndActionTag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1335 --to 1335` (0 hard / 0 soft).

## Seams

FindHighestFloorPoint, FindHighestCeilingSurrounding, FindHighestFloorSurrounding, FindNextLowestCeiling, FindNextHighestCeiling, FindLowestCeilingSurrounding, FindShortestUpperAround, FindModelFloorSector, FindModelCeilingSector, and ActivateInStasisCeiling stay callees. PlaneMoving, GetSpecial, DMovingCeiling::Construct, and StopInterpolation write the thinker fields inside this slice.

## Credit rationale

UNCredited a_ceiling.cpp 216-551 (+336). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of m_argv.cpp 1-335 and whole-file s_sound_eternity.cpp 1-331. Tick through line 215 stays chunk 797. The ActivateInStasisCeiling body from line 552 stays chunk 797.
