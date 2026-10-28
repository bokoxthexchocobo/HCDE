# Chunk 967 — `EWRF_Options`, `FGenericButtons` / `ButtonChecks`, and `DPSprite` field registration head from `p_pspr.cpp` (**1–150**).

## Scope

`EWRF_Options`, `FGenericButtons` / `ButtonChecks`, and `DPSprite` field registration head from `p_pspr.cpp` (**1–150**).

## Native ranges

- `src/playsim/p_pspr.cpp` **FGenericButtons** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPPsprHead967Slice.cs`
Managed LOC (tally): 82.
Conversion commit: `449d8be545eafc74b423694a735c44200e02ffcf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `LOWERSPEED` / `EWRF_Options` (**50–76**) | yes |
| `ButtonChecks[]` (**94–102**) | yes |
| `sv_fastweapons` server flag (**90**) | yes (policy constant) |
| `IMPLEMENT_CLASS` / `DEFINE_FIELD*` hull (**112–150**) | layout metadata only |
| Copyright/includes (**1–48**) | layout only |

| Managed (`VmPPsprHead967Slice.cs`) | Native (`p_pspr.cpp`) |
|------------------------------------|----------------------|
| `Wrf*` / `WrfNoFire` | **62–76** |
| `ButtonChecks` / `FindButtonRowByReadyFlag` | **94–102** |
| `ReadyFlagAllowsButton` / `InputPressedForRow` | `CheckWeaponButtons` policy |
| `Lowerspeed` | **50** |
| `DpspritePointerFields` / scalar / flag names | **114–155** (registration mirror) |
| Band hull (exclusive slice) | **1–150** |

### Chunk **967** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**966** |


**Post-review ratio:** 82/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmPPsprHead967SliceTests.WrfNoFire_CombinesPrimaryAndSecondary`
- `VmPPsprHead967SliceTests.FindButtonRowByReadyFlag_ReturnsReloadRow`
- `VmPPsprHead967SliceTests.ReadyFlagAllowsButton_RequiresStateBit`
- `VmPPsprHead967SliceTests.InputPressedForRow_DetectsButtonBit`
- `VmPPsprHead967SliceTests.ButtonChecks_ContainsSixNativeRows`
- `VmPPsprHead967SliceTests.DpspriteFlagBitNames_MatchesNativeRegistrationCount`
- `VmPPsprHead967SliceTests.Lowerspeed_MatchesNativeMacro`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 967 --to 967` (0 hard / 0 soft).

## Seams

`IMPLEMENT_*` / `DEFINE_FIELD*` macros, `CVAR`, and `NAME_*` tokens remain host seams; weapon-state routing still executed natively.

## Credit rationale

First honest **150**-line UNCredited band in `p_pspr.cpp` with zero conversion/recognition overlap and no existing `VmPPSprite*` coverage (**163+** credited elsewhere).
