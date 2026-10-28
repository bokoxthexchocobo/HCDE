# ACS CallFunction LineAttack case — chunk 76 recognition audit

**Native:** `p_acs.cpp` `ACSF_LineAttack` CallFunction case (6038–6077).

**Managed:** `AcsCallFunctions.TryLineAttack` → `AcsLineAttack.Attack`.

| Case | Handler | Test |
|------|---------|------|
| `ACSF_LineAttack` | `TryLineAttack` | `AcsCallFunctionTests.CallFunc_LineAttack_DamagesActorAlongView` |

**Bucket:** recognition (`managedLoc` 0).

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
