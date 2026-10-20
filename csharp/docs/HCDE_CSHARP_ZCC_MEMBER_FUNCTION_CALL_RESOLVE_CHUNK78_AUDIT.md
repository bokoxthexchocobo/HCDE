# Chunk 78 — `FxMemberFunctionCall::Resolve` (8863–9262)

## Scope

Replaced slices in chunks 71/72/75/77 with a single orchestrator that models the native early-resolve path: class-context gate, static type substitution, static class virtual rebound, const-pointer preprocess, `Super` binding, numeric/ sound/ texture/ sprite/ vector/ quaternion/ string builtins, and dynarray `Size` redirect plus `Push`/`Copy` argument preparation through the pointer-element mismatch check.

## Managed mapping

- `ZccMemberFunctionCallResolver.cs` — primary control flow and builtin rewrite descriptors.
- `ZccMemberFunctionCallModels.cs` — request/argument state carried through the resolver.
- Existing helper types (`ZccMemberFunctionCallResolve`, sprite/vector/dynarray validators) remain thin predicates invoked by the resolver.

## Tests

`ZccMemberFunctionCallResolverTests` — nineteen named tests covering each major native branch in the credited range.

## Verification

- `dotnet build csharp/HCDE.sln -warnaserror`
- `dotnet test csharp/HCDE.sln`
