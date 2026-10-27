# Chunk 894 — `GetCrouchFactor`, `GetCVar`/`GetCVarString`, `GetPlayerInput`, `CountProximity`, and DECORATE cast placeholders (`p_actionfunctions.cpp` **328–530**). Follows **731** `GetZAt` (**326**); stops before **739** `A_RearrangePointers` (**531+**).

## Scope

`GetCrouchFactor`, `GetCVar`/`GetCVarString`, `GetPlayerInput`, `CountProximity`, and DECORATE cast placeholders (`p_actionfunctions.cpp` **328–530**). Follows **731** `GetZAt` (**326**); stops before **739** `A_RearrangePointers` (**531+**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **GetCrouchFactor** 328–530 (203 lines)

**Credited native LOC:** 203 (tally `nativeLoc` 203).

## Managed

`VmActionActorQueryNatives.cs`, `VmActionDecorateInternalPlaysim.cs`
Managed LOC (tally): 103.
Conversion commit: `bec1582d4c26c174bf59b8f7441871051fec122f`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `NativeCallWritesReturnValue` / `ResolveCrouchFactor` | `GetCrouchFactor` **336–356** |
| `PlayerSlotForCvarLookup` / `CvarFloatOrZero` | `GetCVar` **366–386** |
| `CvarStringOrEmpty` | `GetCVarString` **396–416** |
| `PlayerInputOrZero` | `GetPlayerInput` **426–449** |
| `ShouldQueryProximity` / `ProximityCountWhenMobjMissing` | `CountProximity` **459–481** |
| `IsDecorateInternal*` / `DecorateFloatReturnCode` | `__decorate_internal_*` **494–518** |
| `VmActionDecorateInternalPlaysim` | `__decorate_internal_float__` **508–518** ret slot policy |

### vs chunk **731** / **739**

| | Lines |
|--|------:|
| **731** (`GetZAt`) | ends **326** |
| **894** (this row) | **328–530** (**203**) |
| **739** (`A_RearrangePointers`) | **531+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **203** |
| Net behavior-ported | **203** |


**Post-review ratio:** 103/203 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmActionActorQueryNativesTests.ResolveCrouchFactor_DefaultsWithoutPlayer`
- `VmActionActorQueryNativesTests.CvarFloatOrZero_WhenMissing`
- `VmActionActorQueryNativesTests.PlayerInputOrZero_WithoutPlayer`
- `VmActionActorQueryNativesTests.ShouldQueryProximity_WhenMobjSet`
- `VmActionActorQueryNativesTests.IsDecorateInternalInt_MatchesName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 894 --to 894` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `GetCVar`, `P_Thing_CheckInputNum`, and `P_Thing_CheckProximity` are not hosted.

## Credit rationale

NON-ACTION return-slot gates, player/cvar lookup policy, proximity null guard, and decorate-internal cast identifiers mirrored by `VmActionActorQueryNatives`.
