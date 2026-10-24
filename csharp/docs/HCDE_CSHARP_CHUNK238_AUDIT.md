# Chunk 238 — OneKey/Keygroup/Lock check from LOCKDEFS. Retired (2026-10-23): out-of-body credit removed; honest **62** < **150**.

## Scope

OneKey/Keygroup/Lock check from LOCKDEFS. Retired (2026-10-23): out-of-body credit removed; honest **62** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4ba7a76eb71f17ef550204e6c81184bd5e9b4fa6`.

## Padding review (2026-10-21)



### Out-of-body uncredit (238)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **107** |
| **Out-of-body removed** | **45** |
| **Honest** | **62** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 238 --to 238` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
