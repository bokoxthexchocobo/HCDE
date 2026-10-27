# Chunk 580 — `A_SetTics` through `A_DamageTracer` (`p_actionfunctions.cpp` **3994–4151**, contiguous function band). Re-land of retired **543** (no **3751–3762** on **577**).

## Scope

`A_SetTics` through `A_DamageTracer` (`p_actionfunctions.cpp` **3994–4151**, contiguous function band). Re-land of retired **543** (no **3751–3762** on **577**).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_SetTics** 3994–4151 (158 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmActorDoDamage.cs`
Managed LOC (tally): 86.
Conversion commit: `863f5ff4cc46927168bbced47441e18f8d9623e6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `SetTicsOnPsprite` / `ShouldSetActorTics` | `A_SetTics` **3999–4011** |
| `DmssFlags` | `DMSS` **4033–4045** |
| `CheckClass` / `CheckSpecies` / `PassesFilterSpecies` / `AssembleDamageFlags` / `AdjustKillAmount` / `PlanDoDamage` / `HealMagnitude` / `UseInflictorDamageType` | `DoDamage` **4049–4078** (`P_DamageMobj`/`P_GiveBody` seam) |
| `ShouldDamageWhenPresent` | `A_DamageTarget` **4124–4125**, `A_DamageTracer` **4148–4149** |
| (wrappers/param) | `A_DamageSelf` **4087–4103** (`COPY_AAPTR` seam) |

Inter-function comment blocks **4016–4032**, **4081–4086**, **4104–4109**, **4128–4133** sit inside the **3994–4151** band but carry no separate managed helpers.

### vs chunk **543** / **577**

| | Lines |
|--|------:|
| **543** (retired) | — |
| **577** (`DoCheckSpecies`/`DoCheckClass`) | **3751–3762** (excluded) |
| **580** (this row) | **3994–4151** (**158**) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **158** |
| Net behavior-ported | **158** |


**Post-review ratio:** 86/158 managed/native ≈ 54% (src file LOC at HEAD; see tally).

## Tests

- `VmActorDoDamageTests.SetTicsOnPsprite_WhenFound`
- `VmActorDoDamageTests.SetTicsOnActor_WhenFromActor`
- `VmActorDoDamageTests.AssembleDamageFlags_KillAddsNoFactor`
- `VmActorDoDamageTests.PassesDoDamageFilter_BothRequired`
- `VmActorDoDamageTests.PlanDoDamage_HealWhenNegative`
- `VmActorDoDamageTests.ShouldDamageWhenPresent_Target`
- `VmActorDoDamageTests.ShouldDamageWhenPresent_TracerAbsent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 580 --to 580` (0 hard / 0 soft).

## Seams

`FindPSprite`, `COPY_AAPTR`, `P_DamageMobj`, and `P_GiveBody` are not hosted.

## Credit rationale

Tic assignment branches, damage-flag assembly, filter/species gating, kill/heal amount policy, and target/tracer null guards.
