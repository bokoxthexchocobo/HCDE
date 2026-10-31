# Chunk 1343 — Actor light definitions, the Dehacked type alias, and state-light binding from a_dynlightdata.cpp (1-304).

## Scope

Actor light definitions, the Dehacked type alias, and state-light binding from a_dynlightdata.cpp (1-304).

## Native ranges

- `src/r_data/a_dynlightdata.cpp` **GetRealType** 1–304 (304 lines)

**Credited native LOC:** 304 (tally `nativeLoc` 304).

## Managed

`VmDynLightData1343Slice.cs`
Managed LOC (tally): 715.
Conversion commit: `a6a60a209ef583bc3acf070714f534141fa2e9c6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Dehacked alias (**37–45**) | yes |
| Light-default serialize (**56–77**) | yes |
| Light-default array serialize (**79–102**) | yes |
| Apply properties (**111–145**) | yes |
| Level attenuation (**147–160**) | yes |
| Add defaults (**171–194**) | yes |
| Sprite association (**202–233**) | yes |
| State-light init (**249–285**) | yes |
| Add state light (**293–304**) | yes |
| Band hull (**1–304**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GetReplacement | callee; null level and skill lookup off |
| IsDescendantOf | callee; DehackedPickup only |
| FindActor | callee |
| ClassDataAllocator.Alloc | callee; the association is still constructed when the light is missing |
| FName and strncpy | host; case fold and the eight-byte frame buffer |
| FCycler::SetParams | callee; the radius clamp reads GetVal immediately |

A failed array open clears the list and does not end the array. An empty write skips the array. Reading grows with NAME_None definitions and does not delete a slot that shrinks away. Attenuation walks only while the level flag changes, and only definitions whose attenuate field is still -1. Duplicate removal deletes the first same name, then the attenuate byte decides the scale. Sprite match is the first four bytes. NAME_None state slots become null, and an unknown name stays a non-null missing marker. The list ends with a null terminator, then the parsed names are cleared and shrunk.

| Managed (`VmDynLightData1343Slice.cs`) | Native (`a_dynlightdata.cpp`) |
|----------------------------------------|------------------------------|
| Alias, serialize, apply, attenuation, associations, and state lights | **1–304** |
| Band hull (exclusive slice) | **1–304** |

### Chunk **1343** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **304** |
| **Net headline** | **+304** vs the prior chunk |


**Post-review ratio:** 715/304 managed/native ≈ 235% (src file LOC at HEAD; see tally).

## Tests

- `VmDynLightData1343SliceTests.AliasSerializeApplyAndAttenuation`
- `VmDynLightData1343SliceTests.DefaultsAssociationsAndStateLights`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1343 --to 1343` (0 hard / 0 soft).

## Seams

GetRealType asks for the replacement with a null level and skill lookup off, then keeps that replacement only when it is a different DehackedPickup descendant. GetAttenuate is the attenuate byte, so the default -1 still scales both intensities. ApplyProperties saves the previous light type, points the live light at the definition arguments and flags, and copies the offset last. A pulse reads FCycler::SetParams before that copy. Frame names occupy eight bytes. A frame index is stored when the name length is 5 or byte 5 is 0. AddStateLight stores the TArray push index in a 16-bit field and inserts NAME_None only when the state light is 0.

## Credit rationale

UNCredited a_dynlightdata.cpp 1-304 (+304). A full tally line scan shows no overlap. This whole file is the largest remaining hole of at most 400 lines, ahead of codegen.cpp 9535-9836.
