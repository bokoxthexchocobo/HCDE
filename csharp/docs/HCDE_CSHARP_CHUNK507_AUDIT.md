# Chunk 507 — ChaseFlags, A_Chase, chase action glue, A_Face, P_DropItem Retired (2026-10-23): out-of-body credit removed; honest **143** < **150**.

## Scope

ChaseFlags, A_Chase, chase action glue, A_Face, P_DropItem Retired (2026-10-23): out-of-body credit removed; honest **143** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6dd55fa90b568ca26828df9365e5f827837a3259`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `WanderSkipsConversation` / `WanderStandstill` / `WanderReactionTick` / `WanderDirectionTurn` / `WanderRandomTurn` / `ChaseResurrectGate` / `DoChaseFastFlag` / `ChaseFlagBit` / `TurnTowardNegative` / `TurnTowardPositive` / `PainUsesPlayerBranch` / `PickLowHealthPain` / `TypedPainSoundFound` / `TryDamageTypedPain` / `BuildTypedPainKey` | `ChaseFlags` 2268–2281 |
| `A_Chase` | `A_Chase` 2975–2978 |
| `A_Chase` | `A_Chase` 3000–3010 (3009: `return 0;`) |
| `A_DoChaseNative` | `A_DoChaseNative` 3012–3017 (3014: `if ((flags & CHF_RESURRECT) && P_CheckForResurrection(self, false))`; 3015: `return;`) |
| `A_DoChase` | `A_DoChase` 3019–3029 (3028: `return 0;`) |
| `FaceRequiresOther` / `StealthVisdirOnFace` / `ClearsAmbush` / `YawTurnLimited` / `PitchClampEnabled` / `RetargetZToMiddle` / `RetargetZToCenter` / `ApplyFafBottom` / `ApplyFafMiddle` / `ApplyFafTop` / `PitchLimitedTurn` / `PickMidHealthPain` / `PickHighMidPain` / `SourceAimZ` / `DeltaAngleToward` / `UseFullPitchSnap` / `AddPitchOffset` / `AdjustPitch` / `ShadowHandlingAfterYaw` | `A_Face` 3046–3140 (3048: `if (!other)`; 3049: `return;`; 3052: `if (self->flags & MF_STEALTH)`) |
| `A_FaceTarget` | `A_FaceTarget` 3142–3145 |
| `PainAmountSuffix` | `P_DropItem` 3204–3215 (3212: `return retval;`; 3214: `return NULL;`) |

### Out-of-body uncredit (507)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **157** |
| **Out-of-body removed** | **14** |
| **Honest** | **143** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 507 --to 507` (0 hard / 0 soft).

## Seams

`A_DoChase`, `P_CheckForResurrection`, `A_Face_ShadowHandling`, `VMCall`, and `P_RailAttack` are not hosted.

## Credit rationale

Credits chase flag constants, internal `A_Chase` wrapper, decorate chase/do-chase actions, facing math, and drop-item VM dispatch from `p_enemy.cpp` (2268–3215). `A_Wander` stays on chunk463; `A_ChaseNative` body stays on `p-enemy-chunk1`.
