# Chunk 368 — Hexen SetLineID, SaveLineSpecial, FinishLoadingLineDef translucent handling Retired (2026-10-24): thin managed at HEAD — surviving **0** < **77** (50% of honest **153**).

## Scope

Hexen SetLineID, SaveLineSpecial, FinishLoadingLineDef translucent handling Retired (2026-10-24): thin managed at HEAD — surviving **0** < **77** (50% of honest **153**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `341eb9e389ee6a3ad71a9dc3eab8e8f58eb7037f`.

## Padding review (2026-10-21)



### Out-of-body uncredit (368)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **153** |
| **Out-of-body removed** | **0** |
| **Honest** | **153** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 368 --to 368` (0 hard / 0 soft).

## Seams

`Level->tagManager`, `sidetemp`, and live `line_t` mutation are not hosted; managed code mirrors ID extraction, side-special save rules, texel length, and translucency policy.

## Credit rationale

Credits `MapLoader::SetLineID`, `SaveLineSpecial`, `FinishLoadingLineDef`, and `FinishLoadingLineDefs` from `maploader.cpp`.
