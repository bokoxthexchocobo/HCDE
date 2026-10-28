# ACS SaveString / PushFunction / CallStack — chunk 17 audit

## Scope

Execute decoded-only Eternity stack helpers from `p_acs.cpp`: `PCD_SAVESTRING`, `PCD_PUSHFUNCTION`, and a headless subset of `PCD_CALLSTACK` (zero-arg invasion `CallFunc` tags and existing `AcsCallFunctions` with 1–5 stack args). Module script calls (`ScriptFunction` / `PCD_CALL`) remain absent.

**Conversion native LOC credited:** 22 (`PCD_SAVESTRING` 10621–10628, `PCD_PUSHFUNCTION` 7510–7515, `PCD_CALLSTACK` dispatch branch 7526–7533).

**Managed LOC credited:** 52 (`AcsVm` cases + alignment after `PushFunction`).

## Tests

- `AcsEternityStackOpcodeTests.SaveString_PushesGlobalPoolIdFromPrintWork`
- `AcsEternityStackOpcodeTests.PushFunction_PushesFunctionIndexByte`
- `AcsEternityStackOpcodeTests.CallStack_InvokesZeroArgInvasionQueryFromStackTag`

## Verified

`dotnet build -warnaserror`; full `dotnet test` (22,195 passed); `python3 tools/generate_native_managed_matrix.py`.
