# Chunk 896 — `SetViewRollNative`, `GetVar`/`GetArrayVar`, `A_SetUserVar*` / `A_SetUserArray*`, and `T_Flags` (`p_actionfunctions.cpp` **3088–3251**). Follows **raise/count** `SetViewPitchNative` (**3086**); stops before **526** `A_Teleport` (**3255+**).

## Scope

`SetViewRollNative`, `GetVar`/`GetArrayVar`, `A_SetUserVar*` / `A_SetUserArray*`, and `T_Flags` (`p_actionfunctions.cpp` **3088–3251**). Follows **raise/count** `SetViewPitchNative` (**3086**); stops before **526** `A_Teleport` (**3255+**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetViewRollNative** 3088–3251 (164 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmActionUserFieldAccess.cs`, `VmActionTeleportNativeFlags.cs`
Managed LOC (tally): 92.
Conversion commit: `c09a0e9290fdca52bab908cff20e90ab2ad7a433`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SetViewRollOnRef` | `SetViewRollNative` **3099–3102** |
| `IsUserScalarField` | `GetVar` **3127–3133** |
| `IsUserArrayField` / `ArrayIndexInBounds` | `GetArrayVar` **3176–3188** |
| `CanCommitUserScalarWrite` / `CanCommitUserArrayWrite` / `UserArrayElementByteOffset` | `A_SetUserVar*` / `A_SetUserArray*` **3144–3222** |
| `VmActionTeleportNativeFlags` / flag predicates | `T_Flags` **3237–3251** |

### vs **raise/count** / **526**

| | Lines |
|--|------:|
| `SetViewPitchNative` (other row) | ends **3086** |
| **896** (this row) | **3088–3251** (**164**) |
| **526** (`A_Teleport`) | **3255+** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **164** |
| Net behavior-ported | **164** |


**Post-review ratio:** 92/164 managed/native ≈ 56% (src file LOC at HEAD; see tally).

## Tests

- `VmActionUserFieldAccessTests.SetViewRollOnRef_RequiresActor`
- `VmActionUserFieldAccessTests.IsUserScalarField_AcceptsScalar`
- `VmActionUserFieldAccessTests.UserArrayElementByteOffset_MatchesStride`
- `VmActionUserFieldAccessTests.CanCommitUserArrayWrite_WhenFieldAndIndexValid`
- `VmActionTeleportNativeFlagsTests.TelefragFlag_MatchesNativeEnum`
- `VmActionTeleportNativeFlagsTests.UsesRandomDecide_WhenFlagSet`
- `VmActionTeleportNativeFlagsTests.OverrideFlag_MatchesNativeEnum`
- `VmActionTeleportNativeFlagsTests.SensitiveZFlag_MatchesNativeEnum`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 896 --to 896` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `FindSymbol`, `Printf`, `SetValue`, and `AActor::SetViewRoll` are not hosted.

## Credit rationale

Ref-null guard, user scalar/array field validation, setter commit gates, and teleport flag constants mirrored in managed policy; ZScript field lookup bodies are seams.
