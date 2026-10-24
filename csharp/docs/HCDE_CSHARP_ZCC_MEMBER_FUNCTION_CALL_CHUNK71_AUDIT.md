# ZScript FxMemberFunctionCall::Resolve (front) — chunk 71 audit

**Native:** `codegen.cpp` `FxMemberFunctionCall::Resolve` prologue slices only (not the full 200-line band).

**Managed:** `ZccMemberFunctionCallResolve.cs` (`RequiresEnclosingClass`, `TryValidateNonNullArguments`, `TryMapStaticTypeName`, `ConstantContextCallError`).

## Addendum (range trim)

Prior tally credited 8863–9062 (200 native LOC) while managed only implemented the prologue gates above. **Trimmed nativeRanges** to lines that match managed behavior and named tests:

| Lines | Behavior | Test |
|-------|----------|------|
| 8875–8881 | `ctx.Class == nullptr` abort | `RequiresEnclosingClass_RejectsGlobalContext` |
| 8883–8890 | empty `ArgList` slot | `TryValidateNonNullArguments_RejectsEmptySlots` |
| 8899–8902 | `String` / `Quat` static type substitution | `TryMapStaticTypeName_MatchesNativeSubstitutions` |
| 8915–8919 | constant-context call error text | `ConstantContextCallError_MatchesNativeMessage` |

**nativeLoc:** 24. **managedLoc:** 52.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
