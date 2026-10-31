# Chunk 1108 — `DisableLocalRendering`, `ShouldRenderLocally`, `IsInsideVisibleAngles`, and `IsVisibleToPlayer` from `p_mobj.cpp` (**1351–1500**); pivot after `s_sound.cpp` seam (**1107**).

## Scope

`DisableLocalRendering`, `ShouldRenderLocally`, `IsInsideVisibleAngles`, and `IsVisibleToPlayer` from `p_mobj.cpp` (**1351–1500**); pivot after `s_sound.cpp` seam (**1107**).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor visibility and local render** 1351–1500 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjVisibility1108Slice.cs`
Managed LOC (tally): 116.
Conversion commit: `ce02c36725b3d014fb3d0770700a8189ae971ec1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `DisableLocalRendering` console gate (**1353–1354**) | yes |
| `ShouldRenderLocally` (**1374**) | yes |
| `IsInsideVisibleAngles` swap + cone test (**1400–1449**) | yes |
| `IsVisibleToPlayer` team/class scan (**1469–1494**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1334–1348**, **1362–1387** | VM native thunks |
| **1432–1433** | portal displacement host |
| **1501+** | conversation anim outside hull |

| Managed (`VmPMobjVisibility1108Slice.cs`) | Native (`p_mobj.cpp`) |
|----------------------------------------|---------------------|
| Local render + visibility predicates | **1351–1500** |
| Band hull (exclusive slice) | **1351–1500** |

### Chunk **1108** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1107** |


**Post-review ratio:** 116/150 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjVisibility1108SliceTests.RunDisableLocalRendering_OnlyAffectsConsolePlayer`
- `VmPMobjVisibility1108SliceTests.IsInsideVisibleAngles_SkipsWhenMaskDisabled`
- `VmPMobjVisibility1108SliceTests.IsVisibleToPlayer_RequiresTeamMatchInTeamplay`
- `VmPMobjVisibility1108SliceTests.IsVisibleToPlayer_AllowsWhenClassListMatches`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1108 --to 1108` (0 hard / 0 soft).

## Seams

VM `DEFINE_ACTION_FUNCTION` thunks, portal displacement, and `Level->GetConsolePlayer` remain host-injected; **1501+** continues `ConversationAnimation`.

## Credit rationale

UNCredited **`p_mobj.cpp`** **1351–1500** (+150); `s_sound.cpp` EOF **1801–1874** sub-150; `v_font`/**sbarinfo** post-defer bands are past physical EOF.
