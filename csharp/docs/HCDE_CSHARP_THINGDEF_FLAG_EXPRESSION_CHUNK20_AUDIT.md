# thingdef_parse.cpp flag expressions — chunk 20 audit

## Scope

Port `ParseFlagExpressionString` behavior (numeric literal or `|`‑combined named flags, optional parentheses) plus morph and thing-activation flag tables from `thingdef_parse.cpp`.

**Native ranges:** `ParseFlagExpressionString` 512–537, `ParseMorphStyle` 542–563, `ParseThingActivation` 565–585 (69 lines).

**Managed:** `DecorateFlagExpression.cs` (82 lines).

## Tests

`DecorateFlagExpressionTests` (integer literal, morph pipe combine, parenthesized thing activation, unknown name rejection).

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
