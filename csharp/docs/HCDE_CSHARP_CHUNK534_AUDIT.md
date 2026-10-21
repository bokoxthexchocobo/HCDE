# Chunk 534 — FxCompareRel::Resolve prologue, FxSpaceship::Resolve

## Scope

Compare-rel coercion (3876–3933) and spaceship resolve (4704–4752). Numeric compare fold/warnings (3934+) remain chunk **55**.

## Native ranges

- `codegen.cpp` **FxCompareRel::Resolve** 3876–3933 (58 lines)
- `codegen.cpp` **FxSpaceship::Resolve** 4704–4752 (49 lines)

**Credited native LOC:** 107.

## Managed

- `ZccCompareRelResolve.cs`
- `ZccSpaceshipResolve.cs`

## Method → native map

| Managed | Native |
|---------|--------|
| `FirstNumericStringCoerce` / replace helpers | 3889–3898 `StringConstToChar` |
| `TranslationIdCoerce` | 3899–3908 |
| `PlanStringPathCoercions` | 3910–3932 |
| `EntersNumericCompareBody` | branch at 3934 (not credited) |
| `OperatorVersionPolicy` | 4716–4728 |
| `ClassifyOperands` / promote | 4731–4740 |
| `TryFoldConstantSpaceship` | 4742–4749 |

## Tests

- `ZccCompareRelResolveTests.*`
- `ZccSpaceshipResolveTests.*`

## Seams

`StringConstToChar`, `FxStringCast`, `Promote`, and `Emit` are not hosted.

## Credit rationale

Credits compare-rel coercion prologue and spaceship resolve; numeric compare fold remains chunk 55.
