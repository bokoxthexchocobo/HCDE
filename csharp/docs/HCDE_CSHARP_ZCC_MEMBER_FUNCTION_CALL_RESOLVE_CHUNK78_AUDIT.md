# Chunk 78 — `FxMemberFunctionCall::Resolve` (8863–9262)

## Scope

Early-resolve predicate path for member function calls before the generic `isresolved:` lookup. Managed code records **outcomes** (errors, builtin rewrite kind, backing-type remaps, flags) instead of allocating `FxExpression` nodes.

## Faithful reproduction (credited native + managed)

| Native region | Behaviour |
|---------------|-----------|
| 8875–8891 | Reject when `ctx.Class == nullptr`; reject null argument slots |
| 8899–8902 | `String` / `Quat` / `FQuat` static identifier → struct container substitution |
| 8915–8919 | Static class call from constant declaration error |
| 8924–8936 | Descendant static call with virtual method → rebound to `Self` pointer (flags only) |
| 8949–8952 | `const` pointer → `isreadonly` when version ≥ 4.12 |
| 8963–8977 | `Super` requires function context; parent type + non-virtual flag |
| 8998–9003 | `Sound.IsValid` zero-argument gate (4.14.1+) |
| 9017–9022, 9062–9067 | Texture / translation / sprite builtin arity gates |
| 9028–9046, 9073–9107 | Compare / assign opcode selection for texture & sprite IDs (constants) |
| 9122–9127, 9133–9139, 9151–9158 | Vector / `PlusZ` / quaternion unary arity gates |
| 9169–9177 | `String.Length` intrinsic vs struct remap |
| 9181–9188 | Dynarray `Size` → backing member redirect |
| 9220–9273 | Object `Push`/`Insert` type gate; dynarray `Copy`/`Move` backing promotion; pointer element mismatch message |

Managed: `ZccMemberFunctionCallResolve`, sprite/vector/dynarray validators, `ZccDynArraySizeMember`, and the orchestration / `TryDescribe*` helpers in `ZccMemberFunctionCallResolver`.

## Modelled or stubbed (not credited)

- `SAFE_RESOLVE` on `Self` and arguments (8905–8906, 9201–9208, …)
- `FxOutVarDereference` insertion (8954–8958)
- `FindContainerType` / `FindClassMemberFunction` (8901, 8928)
- `goto isresolved` and all post-builtin VM resolve (9048–9052, 9130–9131, …)
- `FxToVector`, `FxCompareRel`, `FxAssign`, `FxVectorBuiltin`, `FxStrLen` node construction
- `afd_override` / `FnOverrides` lookup (9191–9194)
- `Create<PField>` / `RequestAddress` side effects (9245–9260) — only the **decision** to swap/promote is credited

## Tests

`ZccMemberFunctionCallResolverTests` (19 tests) — each maps to a credited faithful region above.

## Verification

- `dotnet build csharp/HCDE.sln -warnaserror`
- `dotnet test csharp/HCDE.sln`

## Addendum (honest LOC)

- **Native credited:** 227 lines in `codegen.cpp` (ranges in `loc_tally.json`; excludes blank/comment-only lines outside those ranges and all stubbed `SAFE_RESOLVE`/expression emission).
- **Managed credited:** 285 LOC across the faithful helper + resolver predicate surface (excludes DTO-only `ZccMemberFunctionCallModels`).
