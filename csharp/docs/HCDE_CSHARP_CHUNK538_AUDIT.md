# Chunk 538 — FxRandom/identifier resolve bands, DEH par/codeptr helpers, A_Weave/LineEffect/WolfAttack math (fidelity-trimmed; chunks 539–540 merged here)

## Scope

FxRandom/identifier resolve bands, DEH par/codeptr helpers, A_Weave/LineEffect/WolfAttack math (fidelity-trimmed; chunks 539–540 merged here)

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxRandom::Resolve** 6206–6213 (8 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandomPick::Resolve** 6310–6315 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandom2::Resolve** 6509–6510 (2 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandomSeed::Resolve** 6588–6589 (2 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6658–6674 (17 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6794–6798 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6839–6843 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6914–6919 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6963–6966 (4 lines)
- `src/gamedata/d_dehacked.cpp` **PatchPars** 2761–2783 (23 lines)
- `src/gamedata/d_dehacked.cpp` **PatchCodePtrs** 2823–2826 (4 lines)
- `src/gamedata/d_dehacked.cpp` **PatchCodePtrs** 2835–2843 (9 lines)
- `src/gamedata/d_dehacked.cpp` **PatchCodePtrs** 2849–2849 (1 lines)
- `src/gamedata/d_dehacked.cpp` **ReplaceSpriteInData** 2911–2921 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Weave** 3491–3523 (33 lines)
- `src/playsim/p_actionfunctions.cpp` **A_LineEffect** 3558–3567 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_WolfAttack** 3607–3646 (40 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmDNetAppendEventRecordsTests.cs`
Managed LOC (tally): 432.
Conversion commit: `57abde1f65e7ff2f889f130e97cbf23a97ca3780`.

## Tests

- `ZccRandomResolveTests.RandomRange_ResolvesMinMax`
- `ZccRandomResolveTests.RandomPick_ResolvesAllChoices`
- `ZccRandomResolveTests.Random2_ResolvesMask`
- `ZccRandomResolveTests.RandomSeed_ResolvesSeedExpr`
- `ZccIdentifierResolveTests.LocalVariable_HighestPriority`
- `ZccIdentifierResolveTests.GlobalConstant_VersionGate`
- `ZccIdentifierResolveTests.RejectsUserinfoCvar`
- `ZccIdentifierResolveTests.MemberField_PrivateAccessDenied`
- `ZccIdentifierResolveTests.MemberFunction_RequiresCallParens`
- `VmDehackedPatchParsCodePtrTests.TryParseParLine_MapFormat`
- `VmDehackedPatchParsCodePtrTests.TryParseParLine_EpisodeFormat`
- `VmDehackedPatchParsCodePtrTests.NormalizeActionSymbol_AddsPrefix`
- `VmDehackedPatchParsCodePtrTests.TryResolveMbfAlias_MatchesAlias`
- `VmDehackedPatchParsCodePtrTests.RemapCPosAttack_DehackedName`
- `VmDehackedPatchParsCodePtrTests.BubblePickupSpriteSort_MovesLeft`
- `VmActorWeaveWolfTests.PlanWeaveStep_AdjustsPosition`
- `VmActorWeaveWolfTests.LineEffectSetsDone_WhenNonRepeating`
- `VmActorWeaveWolfTests.WolfHitChance_ReducesWithDistance`
- `VmActorWeaveWolfTests.WolfDamageAtRange_HalvesTwice`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 538 --to 538` (0 hard / 0 soft).

## Seams

`RESOLVE`, `FxConstant::MakeConstant`, `FindCVar`, `Emit`, `GetLine`, `FindState`, `SetPointer`, `P_TryMove`, `P_ExecuteSpecial`, `P_CheckSight`, and `P_DamageMobj` are not hosted.

## Credit rationale

Credits only native line bands mirrored by managed helpers and named tests — not whole `FxIdentifier::Resolve`, `PatchPars`/`PatchCodePtrs` loops, or action function bodies.

## Port fidelity review

| Native symbol | Prior credit | Finding | Correction |
|---------------|-------------|---------|------------|
| `FxRandom*` / `FxIdentifier::*` | Whole resolve bodies (366 native) | Managed code only modeled operand guards, local dispatch, and member-access predicates | Trimmed to nine codegen bands (55 native) tied to named tests |
| `PatchPars` / `PatchCodePtrs` / `ReplaceSpriteInData` (chunk 539) | Full parser loops | Managed code covers par line parsing, symbol normalize/MBF/CPos remap, pickup bubble sort only | Merged here; 48 native across five DEH bands |
| `A_Weave` / `A_LineEffect` / `A_WolfAttack` (chunk 540) | Full action functions incl. `P_*` | Managed code covers weave bob math, linedef done guard, Wolf hit/damage formulas | Merged here; 83 native across three action bands |

**Corrected totals:** 186 native / 432 managed (tally). Former chunks 539–540 tally rows removed; audits superseded.
