# Chunk 444 — P_GetMaxLineSpecial, P_FindLineSpecial binary search, P_ExecuteSpecial

## Scope

P_GetMaxLineSpecial, P_FindLineSpecial binary search, P_ExecuteSpecial

## Native ranges

- `src/playsim/p_lnspec.cpp` **P_GetMaxLineSpecial** 3878–3881 (4 lines)
- `src/playsim/p_lnspec.cpp` **P_GetLineSpecialInfo** 3889–3896 (8 lines)
- `src/playsim/p_lnspec.cpp` **P_FindLineSpecial** 3906–3937 (32 lines)
- `src/playsim/p_lnspec.cpp` **P_ExecuteSpecial** 3945–3960 (16 lines)
- `src/playsim/p_lnspec.cpp` **DEFINE_ACTION_FUNCTION ExecuteSpecial** 3968–3982 (15 lines)
- `src/d_main.cpp` **D_DoomMain do loop static init** 4701–4775 (75 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmDDoomMainLoopIter.cs`, `VmDDoomMainLoopIterTests.cs`, `VmPLnspecFindExecuteSpecial.cs`, `VmPLnspecFindExecuteSpecialTests.cs`
Managed LOC (tally): 104.
Conversion commit: `9a2262d0b978c13534c899f5d45c2411f58c972d`.

## Tests

- `VmPLnspecFindExecuteSpecialTests.MaxLineSpecialIndex`
- `VmPLnspecFindExecuteSpecialTests.SpecialInTable_Bounds`
- `VmPLnspecFindExecuteSpecialTests.BinarySearchMid_Even`
- `VmPLnspecFindExecuteSpecialTests.TeleportSpecialAlias_Name`
- `VmPLnspecFindExecuteSpecialTests.ExecuteInRange_Valid`
- `VmPLnspecFindExecuteSpecialTests.TeleportAlias_Returns70`
- `VmDDoomMainLoopIterTests.RestartConsoleWhenRestart`
- `VmDDoomMainLoopIterTests.AfterPClassStaticInit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 444 --to 444` (0 hard / 0 soft).

## Seams

Live `LineSpecials[]` function pointers and sorted name table are not hosted.

## Credit rationale

Credits table bounds, info lookup, binary-search steps, TeleportSpecial alias, execute range gate, and VM ExecuteSpecial return wiring from `p_lnspec.cpp`.
