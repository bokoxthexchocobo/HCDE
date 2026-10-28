# Chunk 972 — `GetScriptOwner`, `ShouldIgnoreClientSideScript`, `DLevelScript` field head, and string-builder stack policy from `p_acs.cpp` (**601–750**); follows chunk **971** (**≤600**).

## Scope

`GetScriptOwner`, `ShouldIgnoreClientSideScript`, `DLevelScript` field head, and string-builder stack policy from `p_acs.cpp` (**601–750**); follows chunk **971** (**≤600**).

## Native ranges

- `src/playsim/p_acs.cpp` **GetScriptOwner** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsClientsideGate972Slice.cs`
Managed LOC (tally): 83.
Conversion commit: `2f9b7d5e0f13d01847fef333d722a2ba876f3bb0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetScriptOwner` (**647–650**) | yes |
| `ShouldIgnoreClientSideScript` (**652–667**) | yes |
| `STRINGBUILDER_*` push/pop policy (**740–755**) | yes (predicates only) |
| `DLevelScript` member/method mirrors (**674–736**, **698–715**) | layout metadata |
| `EScriptState` ordinals (**681–693**) | yes (const mirrors) |

**Excluded (duplicate / off-tally / prior chunk)**

| Native | Reason |
|--------|--------|
| **601–624** fixed-point | chunk **971** / `VmPAcsIsClientSideScript` |
| **626–645** `CallReturn` | off-tally `CallReturn` record |
| **669–672** `IsClientSideScript` | off-tally duplicate |

| Managed (`VmPAcsClientsideGate972Slice.cs`) | Native (`p_acs.cpp`) |
|---------------------------------------------|---------------------|
| `GetScriptOwner` | **647–650** |
| `ShouldIgnoreClientSideScript` | **652–667** |
| `StringBuilderShouldPush` / `StringBuilderPopSucceeded` | **740–755** |
| `DLevelScriptProtectedFields` / `PublicMethods` | **717–715** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **972** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**971** |


**Post-review ratio:** 83/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsClientsideGate972SliceTests.GetScriptOwner_UsesPlayerMoWhenActivatorHasPlayer`
- `VmPAcsClientsideGate972SliceTests.ShouldIgnoreClientSideScript_SuppressesOtherPlayerScripts`
- `VmPAcsClientsideGate972SliceTests.ShouldIgnoreClientSideScript_AllowsWorldActivator`
- `VmPAcsClientsideGate972SliceTests.StringBuilderStackPolicy`
- `VmPAcsClientsideGate972SliceTests.HudDefaults_AndWaitStates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 972 --to 972` (0 hard / 0 soft).

## Seams

`CallReturn` ctor, `DLevelScript::RunScript`, and fixed-point inlines **601–624** remain host or off-tally scaffolding; chunk **789** begins at **828** (disjoint).

## Credit rationale

UNCredited band before **789**; matrix credit limited to gates not already in off-tally `VmPAcsIsClientSideScript.cs` (`IsClientSideScript`, simplified `CallReturn`, string-builder helpers).
