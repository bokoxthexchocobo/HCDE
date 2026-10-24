# Chunk 83 — `g_taunt.cpp`

## Scope

HCDE cosmetic taunt sound naming and CCMD cooldown timing (managed subset).

## Faithful reproduction (credited)

| Native lines | Behaviour |
|--------------|-----------|
| 38–50 | `MakeTauntSoundName` (`*taunt` vs `*taunt-variant`) |
| 82–88 | Cooldown comparison and remaining-ms message formatting |

## Not ported (not credited)

- File header, includes, namespace wrapper (lines 1–37, 51–66)
- `HCDETauntPlayOnPawn` / `S_Sound` (58–65)
- Full `CCMD(taunt)` level/pawn checks and `DEM_TAUNT` network write (67–104 except 82–88)

## Managed

`VmHcdeTaunt.cs`

## Tests

- `VmWholeFilePortTests.HcdeTaunt_BuildsCanonicalAndVariantNames`
- `VmWholeFilePortTests.HcdeTaunt_CooldownBlocksBackToBackCalls`

## Addendum (honest LOC)

- **Native credited:** 20 code lines (38–50, 82–88).
- **Managed credited:** 18 LOC in `VmHcdeTaunt.cs`.
