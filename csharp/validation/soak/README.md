# Cross-language soak evidence

This directory stores JSON audit evidence from managed cross-language soak harnesses.

## Harnesses

| Harness | Runner | C++ reference |
| --- | --- | --- |
| `pregame_guest_smoke` | `PregameCrossLanguageSoak` | `csharp/validation/pregame/pregame_guest_smoke.py` |
| `netcode_step12_invasion` | `NetcodeCrossLanguageSoak` | `tests/netcode_step12/netcode_step12_stress.py` |

## Recording evidence

From the repository root, with `hcdeserv` and IWAD configured:

```bash
export HCDE_HCDESERV_PATH=/path/to/hcdeserv
export HCDE_IWAD_PATH=/path/to/doom2.wad
export HCDE_IWAD_CRC=<optional-comma-separated-crcs>
export HCDE_HCDE_CLIENT_PATH=/path/to/hcde   # optional for Step 12 client join

cd csharp
dotnet test --filter CrossLanguageSoakEvidenceArchiveTests
```

Or archive directly from C#:

```csharp
CrossLanguageSoakEvidenceArchive.RecordDefaultEvidence();
```

Evidence files land in `csharp/validation/soak/evidence/` as `{harness}_{timestamp}_{status}.json`. A rollup manifest is written to `csharp/validation/soak/manifest.json`.

When binaries are absent, harnesses record `Skipped` status with a reason instead of failing the xUnit suite.
