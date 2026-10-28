# Chunk 278 — PatchMisc powerups and armor defaults Retired (2026-10-23): out-of-body credit removed; honest **147** < **150**.

## Scope

PatchMisc powerups and armor defaults Retired (2026-10-23): out-of-body credit removed; honest **147** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `387ba440c72cfa88bb5f012281c701c1e3e2c394`.

## Padding review (2026-10-21)



### Out-of-body uncredit (278)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **3** |
| **Honest** | **147** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 278 --to 278` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
