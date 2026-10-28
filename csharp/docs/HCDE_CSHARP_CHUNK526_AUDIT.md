# Chunk 526 — AActor::A_Teleport (merged chunk 525) (merged chunk 525) (merged chunk 525)

## Scope

AActor::A_Teleport (merged chunk 525) (merged chunk 525) (merged chunk 525)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::A_Teleport** 3255–3422 (168 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`VmActionTeleport.cs`, `ZccArrayElementEmit.cs`
Managed LOC (tally): 191.
Conversion commit: `a8e621e40bd7b1bd41c06df71c7e5cff20ccf939`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** `VmActionTeleport` credits `A_Teleport` **3255–3422**; `ZccArrayElementEmit` credits `FxArrayElement::Emit` prefix **8052–8180** (merged chunk 525). Excluded `P_TeleportMove`, fog spawn, and state jump execution.

| Managed | Native |
|---------|--------|
| `BlockedByNoteleport` | `A_Teleport` **3285–3287** (`p_actionfunctions.cpp`) |
| `RandomDecideIndex` / `RandomDecidePasses` / `ShouldAbortRandomDecide` | **3291–3307** |
| `SensitiveZFails` | **3332–3337** |
| `ComputeFinalZ` / `PostMoveZ` | **3341–3349**, **3392** |
| `ShouldForceOrigin` | **3354–3358** |
| `WantsFogBlock` / `EmitSourceFog` / `EmitDestFog` / `SourceFogUsesActor` / `SourceFogUsesSpawnedClass` | **3362–3389** |
| `YawAfterTeleport` / `ZeroVelocityAfterTeleport` / `StateJumpAllowed` | **3394–3399** |
| `TelefragMove` | **3352** |
| `NeedsPointerDeref` / `InternalArrayError` | **8056–8067** (`codegen.cpp`) |
| `UsesSizeAddrField` / `StructMemberUsesDummyVar` / `ClassMemberUsesDummyVar` | **8077–8142** |
| `IsNestedStaticArray` / `EmitBoundR` / `StaticArrayInBounds` / `ResizableArrayNeedsBoundEvenWhenConstant` | **8170–8180**, constant bound **8193–8199** |
| `ShiftBitsForElementSize` / `CanUseShiftInsteadOfMul` / `NeedsShiftEmit` / `MulRkWhenNotPowerOfTwo` | faithful `while (1u << shiftbits < ElementSize)` loop **8257–8263** (honest: outside credited **8052–8180** tail; managed mirror only) |

### Out-of-body uncredit (526)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **297** |
| **Out-of-body removed** | **129** |
| **Honest** | **168** |


**Post-review ratio:** 191/168 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmActionTeleportTests.BlockedByNoteleport_WithoutOverride`
- `VmActionTeleportTests.RandomDecideIndex_Clamps`
- `VmActionTeleportTests.ComputeFinalZ_CeilingClamp`
- `VmActionTeleportTests.ShouldAbortRandomDecide_WhenRollFails`
- `VmActionTeleportTests.SourceFogUsesSpawnedClass_WhenNotActorFog`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 526 --to 526` (0 hard / 0 soft).

## Seams

`P_TeleportMove`, `GetSpotState`, `P_SpawnTeleportFog`, and state jumps are not hosted.

## Credit rationale

Credits A_Teleport spot selection, random-decide gate, sensitive-Z rejection, and fog/angle/velocity flags from `p_actionfunctions.cpp` (3255–3423).
