# ACS CallFunction pointer band — chunk 65 recognition re-audit

**Native:** `p_acs.cpp` `CallFunction` cases below (uncredited lines only).

**Bucket:** recognition (`managedLoc` 0).

| Native case | Managed handler | Test |
|-------------|-----------------|------|
| `ACSF_GetNetID` (5492–5497) | `AcsCallFunctions.TryGetNetId` → `AcsNetId.GetNetId` | `AcsCallFunctionTests.CallFunc_GetNetId_ReturnsActorNetworkId` |
| `ACSF_SetActivator` (5514–5524) | `AcsCallFunctions.TrySetActivator` | `AcsActivatorFailureTests.SetActivatorExistingTidSelectsActor` |
| `ACSF_SetActivatorToTarget` (5526–5548) | `AcsCallFunctions.TrySetActivatorToTarget` | `AcsCallFunctionTests.CallFunc_SetActivatorToTarget_UsesMonsterChaseTarget` |
| `ACSF_SetActivatorByNetID` (5550–5560) | `AcsCallFunctions.TrySetActivatorByNetId` | `AcsCallFunctionTests.CallFunc_SetActivatorByNetId_SwitchesActivator` |
| `ACSF_GetChar` (5578–5590) | `AcsCallFunctions.TryGetChar` | `AcsGetCharByteTests.GetCharReturnsNativeSignedUtf8Byte` |
| `ACSF_GetArmorType` (5630–5645) | `AcsCallFunctions.TryGetArmorType` → `AcsPlayerInventory.ArmorTypeAmount` | `AcsCallFunctionTests.CallFunc_GetArmorType_ReturnsWornAmount` |
| `ACSF_GetArmorInfo` (5647–5681) | `AcsCallFunctions.TryGetArmorInfo` → `AcsPlayerInventory` | `AcsCallFunctionTests.CallFunc_GetArmorInfo_ReturnsMegaSavePercent` |

Dropped: blank line 5620 (`SetAirSupply` closing brace) and `SetSkyScrollSpeed` (credited elsewhere).

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
