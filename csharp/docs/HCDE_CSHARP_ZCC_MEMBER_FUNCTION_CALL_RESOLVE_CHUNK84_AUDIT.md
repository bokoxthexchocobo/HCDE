# Chunk 84 — `FxMemberFunctionCall::Resolve` (9278–9534)

## Scope

Static array `Size`, map argument preparation, and class introspection builtin gates (`GetParentClass`, `GetClassName`, `IsAbstract`).

## Faithful reproduction

Predicate logic for arity, constant static-array size, resizable member size field, map `Insert` object-value gate, map copy/move promotion, and zero-arg class builtins — same pattern as chunk 78 (outcomes, not `FxExpression` trees).

## Not credited

`FxConstant` / `FxGetParentClass` node allocation, `SAFE_RESOLVE`, map-iterator block (9414+).

## Tests

`ZccMemberFunctionCallResolverTests` — static array, map insert, `GetClassName` arity, `IsAbstract`.
