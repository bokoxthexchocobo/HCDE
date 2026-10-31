# Chunk 1383 — Handler install, cvar initialization, and the base cvar construct through ForceSet from c_cvars.cpp (91-292).

## Scope

Handler install, cvar initialization, and the base cvar construct through ForceSet from c_cvars.cpp (91-292).

## Native ranges

- `src/common/console/c_cvars.cpp` **C_InitCVars** 91–292 (202 lines)

**Credited native LOC:** 202 (tally `nativeLoc` 202).

## Managed

`VmCCvars1383Slice.cs`
Managed LOC (tally): 297.
Conversion commit: `4888025a7231e20f377268738a9925cd02074f80`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `C_InstallHandlers` body (**91–92**) | yes |
| `C_InitCVars` scalar pass and flag pass (**94–160**) | yes |
| `C_UninitCVars` (**162–172**) | yes |
| `FBaseCVar` constructor (**174–220**) | yes |
| `FBaseCVar` destructor (**222–236**) | yes |
| Callback and extra-data pointers (**238–266**) | yes |
| `GetHumanString` and `GetHumanStringDefault` (**268–276**) | yes |
| `ForceSet` (**278–292**) | yes |
| Band hull (**91–292**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Search helper and handler signature **65–90** | chunk 145 |
| `SetGenericRep` **293–320** | chunk 162 |
| Derived flag and mask bit scans | callees outside this band |
| `FindCVar` and `DoSet` | callees |

`which` is stored and does not filter the declaration list. Unknown types and the flag or mask types are skipped on the first pass. The second pass builds only flag and mask cvars. A serverinfo name longer than 63 characters aborts with the native fatal text before the cvar is registered. A name of 63 characters is accepted. A flag whose bit value is the serverinfo bit does not take that length check, because the base constructor flags are 0. A hidden cvar is registered and is omitted from tab completion. A null name pointer skips lookup, tab completion, and the map insert, and still receives `CVAR_ISDEFAULT`. An empty name is inserted and tab-completed, and `Find` misses it. Replacing a cvar copies the old favorite value through `ForceSet` while the new flags are still 0, so userinfo and the archive safe value are not updated, then deletes the old cvar without removing the new map entry or its tab command. `ForceSet` notifies userinfo only when the flag is set, send is allowed, the ignore flag is clear, and the handler pointer is set. An archive cvar copies the string rep into the safe value unless the unsafe-context flag is set. The call then clears `CVAR_ISDEFAULT` and `CVAR_UNSAFECONTEXT`. Scalar callbacks run only while callbacks are enabled and the cvar is not already inside a virtual callback. Flag and mask callbacks do not run. Human strings ignore precision. `C_UninitCVars` nulls each map value before delete, so the destructor does not drop the key or the tab command.

| Managed (`VmCCvars1383Slice.cs`) | Native (`c_cvars.cpp`) |
|----------------------------------|------------------------|
| Init, lifetime, and `ForceSet` | **91–292** |
| Band hull (exclusive slice) | **91–292** |

### Chunk **1383** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **202** |
| **Net headline** | **+202** vs the prior chunk |


**Post-review ratio:** 297/202 managed/native ≈ 147% (src file LOC at HEAD; see tally).

## Tests

- `VmCCvars1383SliceTests.InitBuildsScalarsThenFlagAndMask`
- `VmCCvars1383SliceTests.ReplaceKeepsTheOldValueAndForceSetUpdatesUserInfo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1383 --to 1383` (0 hard / 0 soft).

## Seams

Chunk 145 already owns c_cvars.cpp 65-90, including the C_InstallHandlers signature. SetGenericRep at 293 through line 320 stays chunk 162. Lines 1-64 stay below the 150-line floor. FindCVar, DoSet, and the flag and mask bit scans stay callees. The which argument of C_InitCVars is unused. A flag or mask declaration passes 0 and a null callback into FBaseCVar, and the declaration flags word is the bit value. FName lookup is case-insensitive. An empty name becomes NAME_None for Find and is still inserted.

## Credit rationale

UNCredited c_cvars.cpp 91-292 (+202). After chunk 1382 this is the unique largest hole. ObjectStateNode.cpp 1-196 is 196. Gaps longer than 400 lines stay whole and are not sliced.
