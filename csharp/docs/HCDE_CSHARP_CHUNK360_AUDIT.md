# Chunk 360 — am_togglefollow and am_togglegrid console commands Retired (2026-10-23): out-of-body credit removed; honest **12** < **150**.

## Scope

am_togglefollow and am_togglegrid console commands Retired (2026-10-23): out-of-body credit removed; honest **12** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `aa9dcf58487c37dcde087160e94842a217021703`.

## Padding review (2026-10-21)



### Out-of-body uncredit (360)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **12** |
| **Out-of-body removed** | **0** |
| **Honest** | **12** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 360 --to 360` (0 hard / 0 soft).

## Seams

`primaryLevel->automap` and `GStrings` lookups are not hosted; CCMD bodies reduce to toggles and message keys.

## Credit rationale

Credits `CCMD(am_togglefollow)` and `CCMD(am_togglegrid)` toggle plus localized status string selection.
