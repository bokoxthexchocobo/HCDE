# Chunk 1397 — Interpolation class declarations and IMPLEMENT_CLASS registration from r_interpolate.cpp (1-169).

## Scope

Interpolation class declarations and IMPLEMENT_CLASS registration from r_interpolate.cpp (1-169).

## Native ranges

- `src/r_data/r_interpolate.cpp` **DInterpolation registration** 1–169 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmInterpolate1397Slice.cs`
Managed LOC (tally): 246.
Conversion commit: `7eb2af01db741f2bbd90b593897af2ad393a2fde`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Includes (**24–31**) | yes |
| DSectorPlaneInterpolation (**39–61**) | yes |
| DSectorScrollInterpolation (**69–88**) | yes |
| DWallScrollInterpolation (**97–116**) | yes |
| DPolyobjInterpolation (**124–143**) | yes |
| IMPLEMENT_CLASS and pointer table (**152–162**) | yes |
| Band hull (**1–169**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CountInterpolations | starts at line 170 |
| Parameterized constructors | bodies start at lines 379, 547, 654, and 753 |
| Update, Restore, Interpolate, Serialize, PropagateMark | bodies are outside 1-169 |
| DInterpolation parent | Super is not named in this band |

DInterpolation is abstract and publishes Next then Prev, terminated by the all-ones pointer sentinel. The four subclasses are concrete, have no pointer table, and inherit DInterpolation. Their in-band default constructors default-initialize TArray members and leave scalar members indeterminate. InPlaceConstructor exists only for the concrete classes. MyClass starts null, and StaticType returns that pointer.

| Managed (`VmInterpolate1397Slice.cs`) | Native (`r_interpolate.cpp`) |
|----------------------------------------|------------------------------|
| Registration, pointer table, and default construction | **1–169** |
| Band hull (exclusive slice) | **1–169** |

### Chunk **1397** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **169** |
| **Net headline** | **+169** vs the prior chunk |


**Post-review ratio:** 246/169 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmInterpolate1397SliceTests.RegistrationOrderAbstractFlagsAndPointerTable`
- `VmInterpolate1397SliceTests.InPlaceConstructionInitializesOnlyClassMembers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1397 --to 1397` (0 hard / 0 soft).

## Seams

CountInterpolations starts at line 170. Parameterized constructor bodies start at 379, 547, 654, and 753. sizeof is a compiler quantity, not a literal in this band.

## Credit rationale

UNCredited r_interpolate.cpp 1-169 (+169). After chunk 1396 this is the unique largest hole. LocalScopeStateNode.cpp 1-168 is 168. The band is shorter than 400 lines and is not sliced.
