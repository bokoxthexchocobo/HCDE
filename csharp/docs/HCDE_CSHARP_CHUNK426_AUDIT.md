# Chunk 426 — Force fields, glass break, Strife conversation starters Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Force fields, glass break, Strife conversation starters Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `dd27ae334cb6f38f6c6329df5eceeac677328311`.

## Padding review (2026-10-21)



### Out-of-body uncredit (426)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **161** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 426 --to 426` (0 hard / 0 soft).

## Seams

`P_ChangeSwitchTexture`, `NativeStartConversation`, and inventory grants are not hosted.

## Credit rationale

Credits force-field damage, glass switch/junk policy, and conversation/dialog gates from `p_lnspec.cpp`.
