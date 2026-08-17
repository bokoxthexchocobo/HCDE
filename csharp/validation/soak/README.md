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
CrossLanguageSoakEvidenceArchive.RefreshCommittedEvidence(); // prune + re-record committed templates
```

Set `HCDE_REFRESH_SOAK_TEMPLATES=1` and run `RefreshCommittedEvidence_ReplacesStaleHarnessFiles` to refresh the committed `validation/soak/evidence/` tree (used by CI when soak secrets are configured).

When binaries are present, CI also runs `TryRecordPassedValidationEvidence_PassesGateWhenBinariesPresent` with `HCDE_RECORD_PASSED_VALIDATION_EVIDENCE=1` to re-record Passed harness JSON, refresh `manifest.json`, and verify the Passed gate in one step. The workflow then exports a `soak-templates-for-commit` artifact bundle via `ExportCommittedTemplates` (`HCDE_EXPORT_SOAK_TEMPLATES=1`) for copying back into `csharp/validation/soak/`. Commit the refreshed tree after a green soak workflow run.

Evidence files land in `csharp/validation/soak/evidence/` as `{harness}_{timestamp}_{status}.json`. A rollup manifest is written to `csharp/validation/soak/manifest.json`.

## Release checklist gate

When `HCDE_HCDESERV_PATH` and `HCDE_IWAD_PATH` are configured, release validation requires **Passed** status for every harness in `manifest.json`. Evaluate the gate from C#:

```csharp
var gate = CrossLanguageSoakGate.Evaluate();
```

In CI (after evidence refresh), run:

```bash
HCDE_ENFORCE_SOAK_GATE=1 dotnet test --filter CrossLanguageSoakGateTests
```

The main `csharp.yml` workflow also runs `CrossLanguageSoakGateTests` with `HCDE_ENFORCE_SOAK_GATE=1` when `HCDE_HCDESERV_PATH` and `HCDE_IWAD_PATH` repository secrets are configured.

When soak secrets are absent, the gate returns `NotRequired` and does not block merges.

When binaries are absent, harnesses record `Skipped` status with a reason instead of failing the xUnit suite.
