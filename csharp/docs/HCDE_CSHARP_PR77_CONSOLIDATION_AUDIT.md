# PR #77 consolidation audit (`cursor/native-managed-matrix-d8c8`)

Source agent: [bc-a3594051-e33f-5e84-af8d-7cb84490d8c8](bc-a3594051-e33f-5e84-af8d-7cb84490d8c8) / PR #77.

## Merged into `cursor/native-managed-matrix-3df6`

| Area | Action |
| --- | --- |
| `HCDE.ConversionInventory` + tests | Added exploratory matrix builder (not authoritative tally) |
| `AcsClassifyActor.cs` | **New conversion** — `PCD_CLASSIFYACTOR` / `DoClassifyActor` |
| `AcsPlayerInput.cs` | **Refinement** — negative playernum → activator (opcode already credited in chunk 10) |
| `AcsVm.cs` | `ClassifyActor` + `GetPlayerInput` wired via helpers (removed duplicate opcode case) |
| `AcsPlayerInputOpcodeTests` | Named tests for input + classify opcodes |
| `VmMusicPatchPaths` | Already matched PR #77 absolute-path check on this branch |
| `VmIwadCandidateScannerTests` | Skip recursive hidden-file case on non-Windows |
| `HCDE.sln` | ConversionInventory projects |
| `HCDE_CSHARP_MIGRATION.md` | Points to `generate_native_managed_matrix.py` + matrix under `docs/data/` |

## Not imported (single source of truth)

- `csharp/docs/HCDE_NATIVE_MANAGED_MATRIX.{md,json,csv}` from PR #77
- Parallel tally semantics from PR #77 builder

## Conversion credit (no double count)

| Item | Tally |
| --- | ---: |
| `pr77-classify-actor-consolidation` | **66** native / **49** managed |
| `GetPlayerInput` opcode | Already in `acs-absent-opcodes-callfunc-chunk10` — behavior refinement only |

## Tests

`AcsPlayerInputOpcodeTests` (3); `ConversionMatrixTests` (1); full suite green after merge.
