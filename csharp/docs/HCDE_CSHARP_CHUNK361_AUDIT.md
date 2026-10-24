# Chunk 361 — CUSTOM_CVAR wipetype clamp Retired (2026-10-23): out-of-body credit removed; honest **4** < **150**.

## Scope

CUSTOM_CVAR wipetype clamp Retired (2026-10-23): out-of-body credit removed; honest **4** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `9ffbe9bc17403f132276622eade4f31ed513dd3b`.

## Padding review (2026-10-21)



### Out-of-body uncredit (361)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **4** |
| **Out-of-body removed** | **0** |
| **Honest** | **4** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 361 --to 361` (0 hard / 0 soft).

## Seams

Wipe drawer selection stays native; only invalid wipe index clamp is modeled.

## Credit rationale

Credits `CUSTOM_CVAR(wipetype)` bounds check against `wipe_None` and `wipe_NUMWIPES`.
