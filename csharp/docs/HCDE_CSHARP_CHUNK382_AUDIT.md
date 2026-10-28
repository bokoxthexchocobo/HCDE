# Chunk 382 — LoadLineDefs and LoadLineDefs2 vertex/zero-length preprocessing and activation normalization Retired (2026-10-24): thin managed at HEAD — surviving **0** < **75** (50% of honest **150**).

## Scope

LoadLineDefs and LoadLineDefs2 vertex/zero-length preprocessing and activation normalization Retired (2026-10-24): thin managed at HEAD — surviving **0** < **75** (50% of honest **150**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `5e16a3b07b17dd5263f97f52deb61d5d0c87c74e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (382)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 382 --to 382` (0 hard / 0 soft).

## Seams

`TranslateLineDef`, `AllocateSideDefs`, and `SetLineID` bodies are not fully hosted.

## Credit rationale

Credits `MapLoader::LoadLineDefs` and `MapLoader::LoadLineDefs2` from `maploader.cpp`.
