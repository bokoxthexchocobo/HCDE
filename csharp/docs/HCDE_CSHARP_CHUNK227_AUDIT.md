# Chunk 227 — G_SkillProperty, G_SkillName, G_VerifySkill, FSkillInfo copy/replacements/text color. Retired (2026-10-23): `uncredited_range_check --band` OVERLAP with **231** teaminfo gaps; port stays off-tally.

## Scope

G_SkillProperty, G_SkillName, G_VerifySkill, FSkillInfo copy/replacements/text color. Retired (2026-10-23): `uncredited_range_check --band` OVERLAP with **231** teaminfo gaps; port stays off-tally.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a6311f9a1d42a577ef1f3c78fd77e94c3808f752`.

## Padding review (2026-10-21)



### Out-of-body uncredit (227)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **227** |
| **Out-of-body removed** | **28** |
| **Honest** | **199** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 227 --to 227` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
