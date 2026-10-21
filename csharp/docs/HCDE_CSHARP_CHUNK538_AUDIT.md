# Chunk 538 — FxRandom family resolve and FxIdentifier::Resolve/ResolveMember policy bands

## Scope

FxRandom family resolve and FxIdentifier::Resolve/ResolveMember policy bands

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxRandom::Resolve** 6206–6213 (8 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandomPick::Resolve** 6310–6315 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandom2::Resolve** 6509–6510 (2 lines)
- `src/common/scripting/backend/codegen.cpp` **FxRandomSeed::Resolve** 6588–6589 (2 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6658–6674 (17 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6694–6698 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6708–6713 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6728–6733 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6748–6754 (7 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6758–6762 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6774–6778 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6794–6798 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6804–6815 (12 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6824–6827 (4 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6839–6843 (5 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::Resolve** 6849–6851 (3 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6878–6884 (7 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6888–6893 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6910–6912 (3 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6914–6919 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6924–6954 (31 lines)
- `src/common/scripting/backend/codegen.cpp` **FxIdentifier::ResolveMember** 6963–6966 (4 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmDNetAppendEventRecords.cs`, `VmDNetAppendEventRecordsTests.cs`, `ZccIdentifierResolveTests.cs`
Managed LOC (tally): 198.
Conversion commit: `a7bafbadf3ed80e8ca23b7fd2384ad5bc89ac888`.

## Tests

- `ZccRandomResolveTests.RandomRange_ResolvesMinMax`
- `ZccRandomResolveTests.RandomPick_ResolvesAllChoices`
- `ZccRandomResolveTests.Random2_ResolvesMask`
- `ZccRandomResolveTests.RandomSeed_ResolvesSeedExpr`
- `ZccIdentifierResolveTests.LocalVariable_HighestPriority`
- `ZccIdentifierResolveTests.FieldFromConstantDeclaration_Blocked`
- `ZccIdentifierResolveTests.VmFunctionConstant_VersionGate`
- `ZccIdentifierResolveTests.ClassFunctionConstant_VersionGate`
- `ZccIdentifierResolveTests.AmbiguousSelfMember_MarksUnsafe`
- `ZccIdentifierResolveTests.ClassFunction_RequiresParensInClass`
- `ZccIdentifierResolveTests.NoGlobalLookup_StopsResolve`
- `ZccIdentifierResolveTests.GlobalConstant_VersionGate`
- `ZccIdentifierResolveTests.DeprecatedGlobal_WarnsWhenVersionOk`
- `ZccIdentifierResolveTests.InvalidGlobalIdentifier_ErrorPath`
- `ZccIdentifierResolveTests.RejectsUserinfoCvar`
- `ZccIdentifierResolveTests.ShouldDeleteOnUnknownIdentifier`
- `ZccIdentifierResolveTests.MemberConstant_OnContainer`
- `ZccIdentifierResolveTests.MemberField_VersionGate`
- `ZccIdentifierResolveTests.StaticFunction_SeesGlobalIdentifiers`
- `ZccIdentifierResolveTests.MemberField_PrivateAccessDenied`
- `ZccIdentifierResolveTests.ProtectedMember_DeniedOutsideDescendant`
- `ZccIdentifierResolveTests.MemberFunction_RequiresCallParens`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 538 --to 538` (0 hard / 0 soft).

## Seams

`RESOLVE`, `FxConstant::MakeConstant`, `FindCVar`, `Emit`, and VM builtin calls are not hosted.

## Credit rationale

Credits only `codegen.cpp` line bands mirrored by `ZccRandomResolve` / `ZccIdentifierResolve` and named tests — not whole resolve bodies.

## Port fidelity review

`VmDehackedPatchParsCodePtr.cs` and `VmActorWeaveWolf.cs` were removed from this chunk’s `managedSourceFiles` (they are **uncredited** policy helpers; see chunk 539/540 audit notes). Managed LOC now tracks only the two ZScript resolve files above (~128% of credited native).
