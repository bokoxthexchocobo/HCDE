#!/usr/bin/env python3
"""Apply hand-authored auditPaddingReview maps for active chunks 521-547."""
from __future__ import annotations

import json
from pathlib import Path

TOOLS = Path(__file__).resolve().parent

# Active chunks only (523, 525, 543-546 retired; no 534/539/540 files).
AUDIT: dict[int, str] = {
    521: """**Fidelity review (2026-10-21):** Credits `PatchThing` Bits/Mass/`ID #` loop and post-loop finalize in `d_dehacked.cpp` **1632–1906**. Excluded `GetLine`/`FindState`/`statedef.InstallStates`/`PushTouchedActor` call (**1901**), `I_Error` host (**1835–1839**), and `OrgHeights` table read (**1850**).

| Managed | Native (`d_dehacked.cpp`) |
|---------|---------------------------|
| `IsMassKey` / `MassFromVal` | `Mass` branch **1634–1636** |
| `IsBitsKey` / `ParseBitsToken` / `StealthKeywordSetsZdoom` | `Bits` token loop **1638–1676** |
| `ApplyTranslationBits` | **1680–1684** |
| `HasTouchy` / `ClearTouchy` | **1686–1690** |
| `HasBounces` / `ClearBounces` / `ApplyBouncesRemap` / `SelectMissileBounceFlags` / `GrenadeTrailSwap` / `DefaultBounceFactor` / `DefaultWallBounceFactor` / `ApplyNobloodForNonSentient` | **1692–1736** |
| `StealthFriendHack` / `RemapFriend` / `ApplyFriendRemap` | **1738–1748** |
| `TranslucentSetsVchanged2` / `ClearTranslucentBit` / `MarkTranslucentVchanged2` / `ResolveTranslucentAlpha` | **1750–1754**, **1806–1813** |
| `MissileSetsNoteleport` / `ApplyMissileNoteleportFlags` / `ClearNoteleportWhenUnmissiled` / `ShouldClearNoteleport` / `ClearNoteleportFlag` | **1756–1768** |
| `PlayerAlwaysFriendly` / `MergePlayerFriendlyFlags` | **1770–1772** |
| `DoomCompatBounce` | **1778–1781** |
| `IceDamageType` / `FireDamageType` / `GravityQuarter` | **1784–1797** |
| `SetInvisibleRender` / `ClearInvisibleRender` | **1815–1818** |
| `IsIdKey` / `IdKeySetsEdnum` | **1823–1825** |
| `HeightPairInvalid` | **1833–1839** |
| `ShouldRestoreCeilingHeight` | **1846–1851** |
| `ShadowBecameSet` / `ShadowCleared` / `FinalizeShadowStyle` / `DefaultShadowAlpha` / `UseNormalStyleWhenUnshadow` | **1854–1867** |
| `NormalizeSpeed` | **1871–1873** |
| `ShouldPushTouchedActor` | **1876–1878** |
| `ApplyCountKillFlags` / `ClearCountKillFlags` | **1883–1894** |""",
    522: """**Fidelity review (2026-10-21):** `TryBuildFromLegacy` mirrors `HCDEAppendCanonicalEventPayload` switch **3747–3899** (`d_net.cpp`). `VmDNetCheckLevelStart` helpers mirror `CheckLevelStart` **5641–5791** (merged chunk 523). Excluded `HSendPacket`, `TryReleaseLevelStart`, `P_ClearPredictionData` bodies except where cited.

| Managed | Native (`d_net.cpp`) |
|---------|----------------------|
| `TryBuildFromLegacy` | `HCDEAppendCanonicalEventPayload` **3747–3899** |
| `ShouldResendLevelReady` / `LevelReadyPacketSize` | early resend path **5643–5657** |
| `ParseGameticFromBuffer` | **5669** |
| `ShouldAlignGametic` | **5670–5685** |
| `LateJoinSeat` | **5709–5710** |
| `SeatTicDup` | **5712** |
| `ClampDesiredLead` | **5726–5727** |
| `SeatClientTic` | **5728** |
| `SeatSequence` / `MaxSequence` | **5729–5735** |
| `ShouldClampPredictionLead` | **5754–5755** |
| `MaxClientTic` / `ClientTicAboveMax` | **5758–5766** |
| `PlayTic` / `AppliedSequenceAnchor` | **5775–5776** |
| `AckClient` | **5787** |
| `WaitingForStart` | guard **5643** (`LST_WAITING`) |
| `TryReleaseAfterAck` | **5791** (`TryReleaseLevelStart` seam) |
| `ConsolePlayerInRange` | **5730–5731**, **5755** |
| `GameticSnapSafeOnMapChange` / `PreserveSequenceOnMapChange` | comments **5672–5683** (policy only) |
| `AppliedSequenceDiagnosticFix` | **5769–5776** |
| `DebugLevelStartDelay` / `ResetAuthorityWatchdog` / `LogAuthorityStart` | **5667**, **5779**, **5780–5783** (diagnostic seams) |""",
    524: """**Fidelity review (2026-10-21):** Credits `DLevelScript::DoSetActorProperty` switch planning **4166–4405** (`p_acs.cpp`). Excluded live `actor->Die`, `LookupString`, and field mutation.

| Managed | Native (`p_acs.cpp`) |
|---------|---------------------|
| `ActorIsNull` | **4168–4170** |
| `ShouldSkipHealthOnDead` / `HealthShouldDie` | **4175–4188** |
| `AcsToDouble` / `AcsToFloat` | `ACSToDouble` use **4193**, **4201**, etc. |
| `TryMapLegacyRenderStyle` | **4204–4212** |
| `ToggleFlag` / `FlagMaskForProperty` / `IsFlagProperty` | flag cases **4215–4252**, **4240–4251** |
| `RequiresPlayerPawn` | **4235–4238**, **4268–4272**, **4353–4366**, **4389–4396** |
| `RequiresPlayerWithSoundClass` | **4390–4395** |
| `UsesBehaviorStringLookup` | **4279–4308**, **4385–4394** |
| `UsesAcsDouble` | **4192–4201**, **4275–4378** |
| `ApplyFriendlyFlag` / `MonsterTotalBeforeFriendlyToggle` / `MonsterTotalAfterFriendlyToggle` | **4254–4264** |
| `DescribePropertyDispatchNative` | whole `switch` **4172–4404** |""",
    526: """**Fidelity review (2026-10-21):** `VmActionTeleport` credits `A_Teleport` **3255–3422**; `ZccArrayElementEmit` credits `FxArrayElement::Emit` prefix **8052–8180** (merged chunk 525). Excluded `P_TeleportMove`, fog spawn, and state jump execution.

| Managed | Native |
|---------|--------|
| `BlockedByNoteleport` | `A_Teleport` **3285–3287** (`p_actionfunctions.cpp`) |
| `RandomDecideIndex` / `RandomDecidePasses` / `ShouldAbortRandomDecide` | **3291–3307** |
| `SensitiveZFails` | **3332–3337** |
| `ComputeFinalZ` / `PostMoveZ` | **3341–3349**, **3392** |
| `ShouldForceOrigin` | **3354–3358** |
| `WantsFogBlock` / `EmitSourceFog` / `EmitDestFog` / `SourceFogUsesActor` / `SourceFogUsesSpawnedClass` | **3362–3389** |
| `YawAfterTeleport` / `ZeroVelocityAfterTeleport` / `StateJumpAllowed` | **3394–3399** |
| `TelefragMove` | **3352** |
| `NeedsPointerDeref` / `InternalArrayError` | **8056–8067** (`codegen.cpp`) |
| `UsesSizeAddrField` / `StructMemberUsesDummyVar` / `ClassMemberUsesDummyVar` | **8077–8142** |
| `IsNestedStaticArray` / `EmitBoundR` / `StaticArrayInBounds` / `ResizableArrayNeedsBoundEvenWhenConstant` | **8170–8180**, constant bound **8193–8199** |
| `ShiftBitsForElementSize` / `CanUseShiftInsteadOfMul` / `NeedsShiftEmit` / `MulRkWhenNotPowerOfTwo` | faithful `while (1u << shiftbits < ElementSize)` loop **8257–8263** (honest: outside credited **8052–8180** tail; managed mirror only) |""",
    527: """**Fidelity review (2026-10-21):** Credits `convertRawFunctionToFunctionPointer` **1775–1801** and `FxTypeCast::Resolve` early branches **1804–2016** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `PlanRawFunctionPointer` | `convertRawFunctionToFunctionPointer` **1775–1801** |
| `InvalidCastTypes` | **1832–1844** |
| `NullPtrToPointer` | **1851–1853** |
| `NullPtrToIdTypes` / `VersionAtLeast` / `MakeVersion` | **1855–1859** |
| `ClassDescendantAllowsCast` | **1957–1962** |
| `NativeStructToWritablePointer` | **1965–1979** |
| `CompatiblePointerTypes` | `AreCompatiblePointerTypes` **1982–1984** (helper at **267–308**) |
| `CompatibleVectors` | **1994–1999** |
| `ClassifyResolveCast` | composite of **1831–2000** branches |""",
    528: """**Fidelity review (2026-10-21):** Credits `AActor::CheckLOF` guard/aim policy **2358–2516** (`p_actionfunctions.cpp`). Excluded `Trace`/`CheckLOFTraceFunc`/`COPY_AAPTR`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `TargetNullWithoutAllow` | **2453–2468** |
| `ScaleHeightOffset` | **2385–2395** |
| `ScaleWidthOffset` / `ScaleOffsets` | **2397–2400**, **2434** |
| `TraceOriginZ` | **2403–2409** |
| `RangeExceeded` / `ResolveRange` | **2414–2420**, **2485–2487** |
| `AdjustPitchRadians` / `VecToAngle` | **2440–2450** |
| `JumpOnMissSuccess` / `TraceSuccess` | **2501–2502** |
| `MinRangeFail` | **2504–2506** |
| `ShouldLinkHitActor` | **2508–2512** |""",
    529: """**Fidelity review (2026-10-21):** Credits `CheckIfTargetInLOS` **2551–2677** and `CheckIfInTargetLOS` **2688–2759** (`p_actionfunctions.cpp`). Excluded `P_AimLineAttack`/`P_CheckSight`/`Distance3D`.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `UseMaster` / `ResolveNonPlayerHasTarget` | **2566–2568** |
| `UseTracerOrProjectile` / `SeekerOrCheckTracer` | **2570–2575** |
| `FailNullTarget` | **2582–2584**, **2714–2716** |
| `FailDeadTarget` | **2586–2588**, **2719–2721** |
| `InitialDoCheckSight` | **2591**, **2731** |
| `PlayerUsesNoAutoaim` / `PlayerAimBranch` | **2596**, **2604–2619** |
| `FailCombatantOnly` / `FailAlly` | **2624–2631** |
| `FailMaxDistance` | **2635–2637**, **2726–2728** |
| `FailCloseNoJump` / `ApplyCloseDistanceAdjustments` | **2639–2649**, **2733–2743** |
| `SightRequired` | **2655–2657**, **2755–2757** |
| `ClampFov` / `OutsideFov` | **2666–2675** |
| `ResolveInTargetLosHasTarget` | **2698–2711** |
| `InTargetLosFovCheck` | **2746–2753** |
| `PlanCheckIfTargetInLos` / `PlanCheckIfInTargetLos` | composites above (no single native function) |""",
    530: """**Fidelity review (2026-10-21):** Credits count-flag/raise/orientation bands **2767–3028** plus `SetViewAngleNative`/`SetViewPitchNative` **3038–3086** (`p_actionfunctions.cpp`).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CheckFlagResult` | `CheckFlag` **2789–2790** |
| `PlanCountFlags` / `ApplyKillCount` / `CountsAsKill` | `A_ChangeCountFlags` **2801–2824** |
| `ChildUsesMaster` | `A_RaiseChildren` **2860** |
| `SiblingSharesMaster` | `A_RaiseSiblings` **2885–2887** |
| `RefireProbabilityFails` / `RefireTargetInvalid` / `RefireContinuesFiring` | `A_MonsterRefire` **2946–2956** |
| `SetAngleOnRef` / `SetPitchOnRef` / `SetRollOnRef` | **2975–3025** |
| `SetViewAngleOnRef` / `SetViewPitchOnRef` | **3038–3086** |
| `RefNull` | pointer null guards **2975**, **2998**, **3021** |""",
    531: """**Fidelity review (2026-10-21):** `SuperFastHash` mirrors pool hashing used by `ACSStringPool::AddString` **994–1004** and **1007–1016** within credited **945–1274** (`p_acs.cpp`). Hash algorithm body is `SuperFastHash` in `superfasthash.cpp` **50+**; chunk credits call sites only.

| Managed | Native (`p_acs.cpp`) |
|---------|---------------------|
| `SuperFastHash` | `AddString` **996**, **1009** |""",
    532: """**Fidelity review (2026-10-21):** Credits `FxAssign::Resolve` **2685–2860** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `BoolModifyAssignInvalid` | **2694–2700** |
| `NeedsNumericPromotion` | **2705–2722** |
| `NativeStructToMatchingPointer` | **2724–2729** |
| `SameTypeOrRealPointerAlias` / `SkipVectorQuaternionPath` | **2731–2742** |
| `DynArrayAssignPolicy` | **2757–2783** |
| `MapAssignPolicy` | **2785–2811** |
| `MapIteratorAssignPolicy` | **2813–2817** |
| `StructAssignPolicy` | **2819–2833** |
| `FallsThroughToTypeCast` | **2839–2843** |
| `RequiresWritableAddress` / `ModifiableValueFails` | **2846–2850** |
| `NormalizeBitWriteIndex` / `UsesDeprecatedFlagHandler` | **2854–2858** |
| `ClassifyContainerAssign` | composite **2736–2834** |""",
    533: """**Fidelity review (2026-10-21):** Credits `FxAddSub::Resolve` **3309–3396**, `FxMulDiv::Resolve` **3533–3614**, `FxPow::Resolve` **3801–3835** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `CustomAdditionHook` | **3321–3328** |
| `TextureIdPlusInteger` | **3331–3333** |
| `QuaternionBinary` / `VectorBinaryCompatible` / `ClassifyAddSub` | **3335–3358** |
| `FoldFloatBinary` / `FoldIntBinary` / `TryFoldBinaryConstants` | **3361–3387** |
| `VectorModulusForbidden` / `VectorDivisionRequiresVectorLeft` / `QuatTimesVecOrQuat` / `VectorScaleByNumeric` / `NumericScaleVector` / `ClassifyMulDivVector` | **3551–3603** |
| `PowRequiresNumericOperands` | **3812–3816** |
| `PowFoldConstants` (via tests) | **3829–3833** |""",
    535: """**Fidelity review (2026-10-21):** Credits compare-rel **3876–3933**, spaceship **4704–4752**, bit/shift/concat/logical **4497–5082** (`codegen.cpp`). Numeric compare fold remains other chunks.

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `NeedsTranslationCastToInt` / `StringComparisonPath` / `ReplaceLeftViaStringConstToChar` / `ReplaceRightViaStringConstToChar` / `PlanStringPathCoercions` / `CanPromoteNumericCompare` / `EntersNumericCompareBody` | `FxCompareRel::Resolve` **3876–3933** |
| `OperatorVersionPolicy` / `ShouldDeleteOnVersionError` / `ShouldWarnDeprecatedOperator` / `BothNumeric` / `ClassifyOperands` / `FoldSpaceshipCompare` / `ShouldPromoteNumeric` / `NonNumericErrorOperatorName` | `FxSpaceship::Resolve` **4704–4752** |
| `BitOpBoolOperands` / `BitOpNumericOperands` / `FoldBitOp` / `NeedsBoolCast` | `FxBitOp::Resolve` **4497–4538** |
| `RewriteRShiftForUnsignedRight` / `FoldShift` | `FxShift::Resolve` **4592–4629** |
| `ConcatOperandValid` / `ConcatConstantFoldEligible` / `FoldStringConcat` | `FxConcat::Resolve` **4809–4839** |
| `LogicalAndFold` / `LogicalOrFold` / `Flatten` helpers | `FxBinaryLogical::Resolve` **4961–5045**, `Flatten` **5053–5082** |""",
    536: """**Fidelity review (2026-10-21):** Credits `FxDotCross`/`FxTypeCheck`/`FxDynamicCast`/`FxConditional::Resolve` **5142–5442** and `DPlaneWatcher` **1899–1971** (`codegen.cpp`, `p_acs.cpp`).

| Managed | Native |
|---------|--------|
| `DotCrossOperandsCompatible` / `DotCrossResultType` | `FxDotCross::Resolve` **5142–5157** (`codegen.cpp`) |
| `DynamicCastNeedsObjectPointer` | `FxDynamicCast::Resolve` **5301–5314** |
| `NeedsConditionBoolCast` / `ShouldFloatCastBranch` / `TranslationIdBranchValueType` / `ConditionalIncompatible` | `FxConditional::Resolve` **5365–5442** |
| `Construct` | `DPlaneWatcher::Construct` **1899–1928** (`p_acs.cpp`) |
| `PlaneCrossedWatch` | `DPlaneWatcher::Tick` **1945–1971** |""",
    537: """**Fidelity review (2026-10-21):** Credits `FxAbs` **5576–5618**, `FxATan2` **5683–5713**, `FxATan2Vec` **5774–5787**, `FxNew` **5833–5871**, `FxMinMax::Resolve` **5962–6092** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `ClassifyAbs` / `FoldAbsInt` / `FoldAbsFloat` | **5576–5618** |
| `Atan2RequiresNumeric` / `PlanAtan2FloatCasts` / `FoldAtan2Degrees` / `NeedsFloatCast` | **5683–5713** |
| `Atan2Vec` policy (tests) | **5774–5787** |
| `NewScopeMismatch` / `ResolveOuterSide` | **5833–5871** |
| `ClassifyMinMaxOperands` / `MinMaxValueType` / `MinMaxNeedsIntToFloatCast` / `PickMinMaxFloat` / `PickMinMaxInt` / `AllChoicesFoldedToSingle` | **5962–6092** |""",
    538: """**Fidelity review (2026-10-21):** Credits only thin native bands listed in `chunk538.json` `nativeRanges` (random resolve heads + `FxIdentifier` policy slices **6206–6966**). Whole resolve bodies are not hosted.

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `ValidateRandomRange` | `FxRandom::Resolve` **6206–6213** |
| `RandomPickAllResolved` / `RandomPickTypeUniform` / `PickIndexForEmit` / `RandomPickSwitchUpperBound` | `FxRandomPick::Resolve` **6310–6315** (+ emit policy) |
| `ClassifyLocal` | `FxIdentifier::Resolve` **6658–6674** |
| `ConstantDeclarationFieldAccess` / `FieldFromConstantContext` | **6694–6698** |
| `VmFunctionConstantAllowed` | **6708–6713** |
| `ClassConstantLookup` / `ClassFunctionWithoutParens` | **6728–6762** |
| `AmbiguousSelfMember` | **6748–6754** |
| `PlanIdentifierResolve` / `GlobalVersionAccessible` / `DeprecatedGlobalWarning` / `ShouldDeleteOnError` | **6774–6851** |
| `MemberResolvePath` / `MemberFieldVersionOk` / `StaticFunctionSeesGlobal` / `PrivateMemberDenied` / `ProtectedMemberDenied` / `ResolveMemberDeletesObject` | `ResolveMember` **6878–6966** |""",
    541: """**Fidelity review (2026-10-21):** Credits `HCDEDemCommandName` **3902–3923**, `HCDEAppendEventRecords` **3932–4026**, `HCDEReadCanonicalString` **4028–4051**, legacy weapon/CVar append **4062–4109** (`d_net.cpp`).

| Managed | Native (`d_net.cpp`) |
|---------|----------------------|
| `DemCommandName` | `HCDEDemCommandName` **3902–3923** |
| `ValidateAppendPreconditions` / `AppendEventRecords` / `EventCountOverflow` / `ShouldDropClientInputEvent` / `PayloadTooLarge` / `WriteEventCountBE16` / `WritePayloadSizeBE16` | `HCDEAppendEventRecords` **3932–4026** |
| `TryReadCanonicalString` | **4028–4051** |
| `TryAppendLegacyWeaponIndex` | **4062–4072** |
| `TryAppendLegacyCVarChange` / `LegacyCVarHeaderByte` | **4074–4109** |
| `TryReadByte` / `TryAppendOutputByte` / `SinChangedXorEvent` | helper seams inside **3932–4026** |""",
    542: """**Fidelity review (2026-10-21):** Credits `HCDEAppendLegacyRunArgs` **4111–4136** and `HCDEAppendLegacyEventPayload` switch **4144–4299** (`d_net.cpp`). `VmActorDoDamage`/`VmActorDoKill` playsim bands are tested but not padding-listed. Excluded `.inl` tail and live buffer append.

| Managed | Native (`d_net.cpp`) |
|---------|----------------------|
| `TryPlanLegacyRunArgs` / `MaskRunArgCount` / `UsesNamedRunArgs` | **4111–4136** |
| `RunArgPayloadBytes` / `UsesRunArgs` | **4256–4269** |
| `IsZeroPayloadEvent` | **4144–4161** |
| `SetInvTrailingBytes` | **4162–4171** |
| `GiveTakeCheatTrailingBytes` | **4173–4177** |
| `NetEventTrailingBytes` | **4184–4187** |
| `Summon2TrailingBytes` | **4199–4204** |
| `ChangeMap2LeadingBytes` / `WarpCheatFieldBytes` | **4211–4230** |
| `ConvReplyFieldBytes` | **4232–4254** |
| `UsesLegacyCVarChange` | **4256–4269** |
| `SetSlotNeedsPnumPrefix` / `SetSlotLeadingFieldBytes` / `TryReadSetSlotHeader` | **4275–4299** |
| *(playsim test seam, no padding helpers)* | `A_SetTics` **3999–4012**, `DoDamage` **4047–4079**, `KILS` **4246–4255**, `DoKill` **4257–4286** (`p_actionfunctions.cpp`) |""",
    547: """**Superseded chunk numbers:** 545 (retired; health/CheckBlock credit lives in chunk547).

**Fidelity review (2026-10-21):** Credits health/chase/CheckBlock **4652–4888** and `DoRemove` filter **4430–4463** (`p_actionfunctions.cpp`). `VmDNetClearBuffers` mirrors `Net_ClearBuffers` defaults **4337–4417** (`d_net.cpp`) at tally seam (not in `nativeRanges`).

| Managed | Native |
|---------|--------|
| `RemoveMisc` / `RemoveMonster` / `RemoveMissile` / `RemoveEverything` / `PassesRemoveFilter` | `DoRemove` **4443–4462** (`p_actionfunctions.cpp`) |
| `ShouldSkipHealthPointer` | **4660–4662** |
| `ResolveHealthAssignment` / `AssignMobjHealth` / `AssignPlayerHealth` | **4668–4678** |
| `ShouldResetPlayerHealth` / `ShouldResetMobjHealth` | **4703–4709** |
| `ShouldSkipHealthPointer` (species) | **4726–4728** |
| `ClampChaseThreshold` / `AssignChaseThreshold` | **4756–4759** |
| `MergeCheckBlockAngle` | **4821–4823** |
| `PlanCheckBlockOffset` | **4829–4837** |
| `PlanDropoffCheckFlags` | **4852–4854** |
| `CheckBlockReturnValue` | **4866–4888** |
| `ShouldApplyBlockingPointer` / `BlockingSetterIsPointer` | **4871–4879**, **4796** |
| `ClientSlotPlayingInGameAfterClearNative` | `playeringame[i]=false` **4343** (`d_net.cpp`) |
| `ClientSlotAppliedSequenceAfterClearNative` | **4355** |
| `ClientSlotAuthorityEventReplayNextIdAfterClearNative` | **4367** |
| `GlobalPendingLocalHealthRepairValidAfterClearNative` | **4405** |
| `GlobalNetGameAfterClearNative` | **4417** |
| `GlobalPredictionEndCapTicAfterClearNative` | **4408** |""",
}


def main() -> None:
    for num, audit in sorted(AUDIT.items()):
        path = TOOLS / f"chunk{num}.json"
        data = json.loads(path.read_text(encoding="utf-8"))
        chunk = data[0]
        chunk["auditPaddingReview"] = audit.strip() + "\n"
        path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
        print(f"updated chunk{num}.json")


if __name__ == "__main__":
    main()
