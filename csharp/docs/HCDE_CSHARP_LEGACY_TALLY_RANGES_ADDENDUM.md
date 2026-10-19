# Legacy tally ranges addendum

## Policy

- Removed `LegacyHeadlineChunksWithoutRanges` from `LocTallyIntegrityTests`; every `nativeLoc > 0` chunk now requires `nativeRanges` with named tests.
- `conversionManagedCommit` (full git SHA) is required for conversion chunks with native credit; recognition chunks must not carry it and keep `managedLoc = 0`.
- Chunk 19 (`P_PlaySpawnSound`) reclassified to **recognition** (managed code predated the chunk).

## Legacy headline chunks backfilled (18)

| Chunk | Notes |
| --- | --- |
| `acs-random-getcvar-polywait` | `DLevelScript::Random` + opcode cases |
| `combat-trace-ordering` | `p_trace.cpp` 200–479 |
| `acs-map-mutations` | `ChangeFlat` / spawn helpers + opcodes |
| `line-specials-plat-generic-elevator` | `p_lnspec.cpp` FUNC ranges (plat/generic/elevator) |
| `line-specials-gap-closure` | sector env + closure specials |
| `line-special-feasible-gaps` | waggle / wind / spawn-facing subset |
| `sector-changeflags-gap-audit` | `LS_Sector_ChangeFlags` |
| `acs-callfunc-wire-dispatch-udmf` | UDMF `CallFunc` wires |
| `action-combat-port` / `action-combat-dispatch-batch2` | `DEFINE_ACTION_FUNCTION` spans |
| `action-dispatch-damage-wiring` | dispatch wiring + explode tests |
| `action-explode-radius-give` | `DoRadiusGive` + radius damage loop |
| `thing-projectile-175-178` | thing projectile line specials |
| `acs-hot-callfunc-behavior` / `acs-air-userarray-spawnspot` | hot `CallFunc` cases |
| `p-enemy-chunk1` / `p-map-radius-chunk1` | audited native spans |

Backfill applied via `csharp/tools/apply_legacy_loc_tally_backfill.py`; `nativeLoc` recomputed from range sums.

## Verified

`python3 csharp/tools/generate_native_managed_matrix.py`; `dotnet build -warnaserror`; `dotnet test`; `LocTallyIntegrityTests` green.
