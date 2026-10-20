# ZScript FxMemberFunctionCall::Resolve (texture/sound) — chunk 72 audit

**Native:** readonly const-pointer flag and zero-argument `Sound`/`TextureID` builtin arity checks.

**Managed:** `ShouldMarkReadonlyConstPointer`, `TryValidateSoundIsValid`, `TryValidateZeroArgBuiltin`.

## Addendum (range trim)

Prior tally credited 9063–9262 (200 native LOC) for sprite/vector/dynarray logic not ported in C#. **Trimmed** to:

| Lines | Behavior | Test |
|-------|----------|------|
| 8949–8952 | const struct pointer → `isreadonly` (4.12+) | `ShouldMarkReadonlyConstPointer_Requires412` |
| 8998–9003 | `Sound.IsValid` arity | `TryValidateSoundIsValid_RejectsExtraArgs` |
| 9017–9022 | `TextureID` zero-arg builtins arity | `TryValidateZeroArgBuiltin_RejectsTextureIsNullArity` |

**nativeLoc:** 16. **managedLoc:** 18.

Verified: `dotnet build -warnaserror`; `dotnet test csharp/HCDE.sln`.
