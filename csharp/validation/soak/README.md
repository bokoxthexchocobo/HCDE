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

The main `csharp.yml` workflow also runs `CrossLanguageSoakGateTests` with `HCDE_ENFORCE_SOAK_GATE=1` when `HCDE_HCDESERV_PATH` and `HCDE_IWAD_PATH` repository secrets are configured. The main workflow includes dedicated `Evaluate_RequiresPassedManifestWhenSecretsConfigured`, `Evaluate_EnforcesDualFreshnessWhenSecretsConfigured`, `ExportCommittedTemplates_RejectsStaleCommittedManifest`, `ApplyExportedTemplates_RejectsStale`, `Evaluate_EnforcesEvidenceFreshnessWhenSecretsConfigured`, `EvaluateManifestStaleness_PassesWhenWithinMaxAge`, `Evaluate_FailsWhenManifestIsStale`, `Evaluate_FailsWhenEvidenceFileIsStale`, `Evaluate_FailsWhenEvidenceFileIsMissing`, `Evaluate_FailsWhenManifestContainsSkippedHarness`, `EvaluateEvidenceFreshness_PassesWhenWithinMaxAge`, `EvaluateCommittedDualFreshness_FailsWhenManifestIsStale`, `ApplyExportedTemplates_RejectsStaleBundleEvidence`, `ApplyExportedTemplates_RejectsStaleExportManifest`, `Evaluate_ReturnsNotRequiredWhenSecretsMissing`, `TryApplyExportedTemplatesFromEnvironment_ReturnsNotRequiredWhenUnset`, and `TryRecordPassedValidationEvidence_ReturnsNotRequiredWhenSecretsMissing` steps via `CrossLanguageSoakGate.Evaluate`, `CrossLanguageSoakGate.EvaluateCommittedDualFreshness`, `CrossLanguageSoakEvidenceArchive.ExportCommittedTemplates`, `CrossLanguageSoakEvidenceArchive.ApplyExportedTemplates`, `CrossLanguageSoakGate.EvaluateEvidenceFreshness`, `CrossLanguageSoakGate.EvaluateManifestStaleness`, `CrossLanguageSoakGate.Evaluate`, `CrossLanguageSoakEvidenceArchive.TryApplyExportedTemplatesFromEnvironment`, and `CrossLanguageSoakEvidenceArchive.TryRecordPassedValidationEvidence`. The main workflow also runs `ExportCommittedTemplates_CopiesManifestAndEvidence` with `HCDE_EXPORT_SOAK_TEMPLATES=1` to verify export bundles copy manifest and evidence files from the committed soak tree. The main workflow also runs `ApplyExportedTemplates_CopiesBundleIntoCommittedTree` to verify export bundles copy manifest and evidence files into a committed soak tree. The main workflow also runs `RefreshCommittedEvidence_CopiesManifestAndEvidence` to verify refresh prunes stale harness JSON and re-records manifest plus evidence files in the committed soak tree. The main workflow also runs `RecordEvidence_CopiesManifestAndEvidence` to verify record-evidence writes manifest and harness JSON files into a committed soak tree. The main workflow also runs `RecordEvidence_CopiesHarnessJsonFiles` to verify record-evidence copies harness JSON files into a committed soak evidence directory. The soak workflow (`csharp-cross-language-soak.yml`) runs `Evaluate_EnforcesDualFreshnessWhenSecretsConfigured` and `ExportCommittedTemplates_RejectsStaleCommittedManifest` before export, and `ApplyExportedTemplates_RejectsStale` before apply. Both `csharp.yml` and `csharp-cross-language-soak.yml` set `HCDE_SOAK_MANIFEST_MAX_AGE_DAYS` and `HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS` to 8 when enforcing the gate. When soak secrets are absent, `Evaluate_ReturnsNotRequiredWhenSecretsMissing` verifies the gate returns `NotRequired`. When `HCDE_APPLY_SOAK_TEMPLATES` is unset, `TryApplyExportedTemplatesFromEnvironment_ReturnsNotRequiredWhenUnset` verifies apply returns `NotRequired`. When soak secrets are absent, `TryRecordPassedValidationEvidence_ReturnsNotRequiredWhenSecretsMissing` verifies record-evidence returns `NotRequired`. `ShouldEnforceInCi_ReturnsTrueWhenEnvSet` verifies the soak enforcement flag is honored when `HCDE_ENFORCE_SOAK_GATE=1`. `RecordEvidence_SkipsWhenHcdeservOrIwadMissing` verifies skipped harness evidence is recorded when soak secrets are absent. `RunAll_SkipsWhenHcdeservOrIwadMissing` verifies the soak suite skips all harnesses when soak secrets are absent. `ExportCommittedTemplates_WritesCiArtifactBundle` verifies the export helper writes a CI artifact bundle when `HCDE_EXPORT_SOAK_TEMPLATES=1`. `ApplyExportedTemplates_WritesCommittedSoakTree` verifies the apply helper copies an export bundle into the committed soak tree when `HCDE_APPLY_SOAK_TEMPLATES=1` (the main workflow exports first into the same temp directory, then applies). `RecordEvidence_WritesManifestWithHarnessStatuses` verifies `RecordEvidence` writes a manifest with harness statuses for CI validation. `RecordEvidence_WritesHarnessJsonFiles` verifies `RecordEvidence` writes harness JSON files for CI validation. `RecordValidationSkippedEvidence_WhenRequested` verifies Skipped soak evidence can be recorded when `HCDE_RECORD_VALIDATION_EVIDENCE=1`. The main workflow also runs `RecordValidationSkippedEvidence_CopiesHarnessJsonFiles` to verify record-validation-skipped evidence copies skipped harness JSON files into a committed soak evidence directory. `RecordValidationPassedEvidence_WhenRequested` verifies Passed soak evidence can be recorded when `HCDE_RECORD_VALIDATION_EVIDENCE=1` and soak secrets are configured. The main workflow also runs `RecordValidationPassedEvidence_CopiesHarnessJsonFiles` to verify record-validation-passed evidence copies harness JSON files into a committed soak evidence directory when soak secrets are absent. `TryRecordPassedValidationEvidence_PassesGateWhenBinariesPresent` verifies Passed soak evidence can be re-recorded when `HCDE_RECORD_PASSED_VALIDATION_EVIDENCE=1` and soak secrets are configured. The main workflow also runs `TryRecordPassedValidationEvidence_CopiesHarnessJsonFiles` to verify try-record-passed-validation evidence copies Passed harness JSON files into a committed soak evidence directory when soak secrets are configured. `RefreshCommittedEvidence_ReplacesStaleHarnessFiles` verifies stale harness JSON is pruned and re-recorded when `HCDE_REFRESH_SOAK_TEMPLATES=1`.

When soak secrets are absent, the gate returns `NotRequired` and does not block merges.

Committed templates must also be fresh: `CrossLanguageSoakGate.Evaluate` rejects manifests older than `CrossLanguageSoakManifest.DefaultMaxManifestAgeDays` (8 days). Override with `HCDE_SOAK_MANIFEST_MAX_AGE_DAYS` when enforcing the gate in CI.

Each manifest `EvidenceFile` entry must exist under `csharp/validation/soak/evidence/` and be newer than `CrossLanguageSoakManifest.DefaultMaxEvidenceAgeDays` (8 days). Override with `HCDE_SOAK_EVIDENCE_MAX_AGE_DAYS` when enforcing the gate in CI.

When binaries are absent, harnesses record `Skipped` status with a reason instead of failing the xUnit suite.
