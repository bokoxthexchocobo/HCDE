# Chunk 240 — a_keys LOCKDEF remainder. Retired (2026-10-23): out-of-body credit removed; honest **85** < **150**.

## Scope

a_keys LOCKDEF remainder. Retired (2026-10-23): out-of-body credit removed; honest **85** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7b2803be2452cfe3b6d2ad83f8e9ce57b041081b`.

## Padding review (2026-10-21)



### Out-of-body uncredit (240)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **85** |
| **Out-of-body removed** | **0** |
| **Honest** | **85** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 240 --to 240` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
