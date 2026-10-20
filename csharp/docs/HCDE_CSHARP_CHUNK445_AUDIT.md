# Chunk 445 — D_DoomMain_Internal restart-loop class init and IWAD/mod resolution pass

## Scope

D_DoomMain_Internal restart-loop class init and IWAD/mod resolution pass

## Native ranges

- `src/d_main.cpp` **do loop PClass/PType init** 4658–4664 (7 lines)
- `src/d_main.cpp` **IWAD/mod iteration body** 4666–4761 (96 lines)

**Credited native LOC:** 103 (tally `nativeLoc` 103).

## Managed

`VmDDoomMainIwadIteration.cs`, `VmDDoomMainIwadIterationTests.cs`, `VmDDoomMainLoopIter.cs`, `VmDDoomMainLoopIterTests.cs`
Managed LOC (tally): 69.
Conversion commit: `6d773cbb1c49882e1370a9025bbb96d095211d44`.

## Tests

- `VmDDoomMainLoopIterTests.AfterPClassStaticInit`
- `VmDDoomMainIwadIterationTests.SharewareBlocksPwads_WhenModsPresent`
- `VmDDoomMainIwadIterationTests.AbortLoopWhenInitGameFailed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 445 --to 445` (0 hard / 0 soft).

## Seams

`FIWadManager`, `D_InitGame`, and filesystem iterators are not hosted.

## Credit rationale

Credits PClass/PType static init, IWAD rediscovery, shareware pwad gate, SteamAppId digit validation, and D_InitGame status handling from `d_main.cpp`.
