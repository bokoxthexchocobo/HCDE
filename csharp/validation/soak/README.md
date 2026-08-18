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

When binaries are present, CI also runs `TryRecordPassedValidationEvidence_PassesGateWhenBinariesPresent` with `HCDE_RECORD_PASSED_VALIDATION_EVIDENCE=1` to re-record Passed harness JSON, refresh `manifest.json`, and verify the Passed gate in one step. The workflow then exports a `soak-templates-for-commit` artifact bundle via `ExportCommittedTemplates` (`HCDE_EXPORT_SOAK_TEMPLATES=1`) for copying back into `csharp/validation/soak/`. On weekly scheduled runs, CI applies the artifact bundle back into the repository via `ApplyExportedTemplates` (`HCDE_APPLY_SOAK_TEMPLATES=1`) and commits refreshed templates when they change. `ApplyExportedTemplates` rejects export bundles whose `manifest.json` is older than `HCDE_SOAK_MANIFEST_MAX_AGE_DAYS` or whose `EvidenceFile` entries are missing or older than `HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS` before copying into `csharp/validation/soak/`. Commit the refreshed tree after a green soak workflow run.

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

The main `csharp.yml` workflow also runs `CrossLanguageSoakGateTests` with `HCDE_ENFORCE_SOAK_GATE=1` when `HCDE_HCDESERV_PATH` and `HCDE_IWAD_PATH` repository secrets are configured. The main workflow includes dedicated `Evaluate_RequiresPassedManifestWhenSecretsConfigured`, `Evaluate_EnforcesDualFreshnessWhenSecretsConfigured`, `ExportCommittedTemplates_RejectsStaleCommittedManifest`, `ApplyExportedTemplates_RejectsStale`, `Evaluate_EnforcesEvidenceFreshnessWhenSecretsConfigured`, `EvaluateManifestStaleness_PassesWhenWithinMaxAge`, `Evaluate_FailsWhenManifestIsStale`, `Evaluate_FailsWhenEvidenceFileIsStale`, `Evaluate_FailsWhenEvidenceFileIsMissing`, `Evaluate_FailsWhenManifestContainsSkippedHarness`, `EvaluateEvidenceFreshness_PassesWhenWithinMaxAge`, and `EvaluateCommittedDualFreshness_FailsWhenManifestIsStale` steps via `CrossLanguageSoakGate.Evaluate`, `CrossLanguageSoakGate.EvaluateCommittedDualFreshness`, `CrossLanguageSoakEvidenceArchive.ExportCommittedTemplates`, `CrossLanguageSoakEvidenceArchive.ApplyExportedTemplates`, `CrossLanguageSoakGate.EvaluateEvidenceFreshness`, `CrossLanguageSoakGate.EvaluateManifestStaleness`, and `CrossLanguageSoakGate.Evaluate`. The soak workflow (`csharp-cross-language-soak.yml`) runs `Evaluate_EnforcesDualFreshnessWhenSecretsConfigured` and `ExportCommittedTemplates_RejectsStaleCommittedManifest` before export, and `ApplyExportedTemplates_RejectsStale` before apply. Both `csharp.yml` and `csharp-cross-language-soak.yml` set `HCDE_SOAK_MANIFEST_MAX_AGE_DAYS` and `HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS` to 8 when enforcing the gate.

When soak secrets are absent, the gate returns `NotRequired` and does not block merges.

Committed templates must also be fresh: `CrossLanguageSoakGate.Evaluate` rejects manifests older than `CrossLanguageSoakManifest.DefaultMaxManifestAgeDays` (8 days). Override with `HCDE_SOAK_MANIFEST_MAX_AGE_DAYS` when enforcing the gate in CI.

Each manifest `EvidenceFile` entry must exist under `csharp/validation/soak/evidence/` and be newer than `CrossLanguageSoakManifest.DefaultMaxEvidenceAgeDays` (8 days). Override with `HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS` when enforcing the gate in CI.

When binaries are absent, harnesses record `Skipped` status with a reason instead of failing the xUnit suite.
