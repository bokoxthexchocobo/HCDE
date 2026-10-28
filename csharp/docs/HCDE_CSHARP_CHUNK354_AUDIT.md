# Chunk 354 — Strife/Raven palettes and AM_initColors preset routing Retired (2026-10-23): out-of-body credit removed; honest **60** < **150**.

## Scope

Strife/Raven palettes and AM_initColors preset routing Retired (2026-10-23): out-of-body credit removed; honest **60** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `aaa59edac5b4e001b181944fab947999c5188b91`.

## Padding review (2026-10-21)



### Out-of-body uncredit (354)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **116** |
| **Out-of-body removed** | **56** |
| **Honest** | **60** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 354 --to 354` (0 hard / 0 soft).

## Seams

CVAR `initFromCVars` and live `AMColors` globals are not hosted; managed code mirrors preset selection, mod overlay routing, and static palette bytes.

## Credit rationale

Credits `StrifeColors`/`RavenColors` tables, `AM_ClearColorsets`, and the `AM_initColors` overlay/custom/preset switch through Raven/Doom/Strife `initFromColors` arms.
