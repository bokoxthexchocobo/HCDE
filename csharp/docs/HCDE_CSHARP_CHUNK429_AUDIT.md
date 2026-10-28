# Chunk 429 — Exit, teleport variants, line teleport, thrust XY/Z, poly stop sound (expanded bands) Retired (2026-10-23): out-of-body credit removed; honest **9** < **150**.

## Scope

Exit, teleport variants, line teleport, thrust XY/Z, poly stop sound (expanded bands) Retired (2026-10-23): out-of-body credit removed; honest **9** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `6279160ed3153d2edc84bc0631f7eb3c83a2e60e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (429)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **212** |
| **Out-of-body removed** | **203** |
| **Honest** | **9** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 429 --to 429` (0 hard / 0 soft).

## Seams

`EV_Teleport`, `EV_SilentLineTeleport`, `SN_StopSequence`, and live `AActor` velocity are not hosted.

## Credit rationale

Credits exit-good checks, teleport flag/orientation policy, silent line teleport, thrust clamping, and Z-thrust set/add modes from `p_lnspec.cpp`.
