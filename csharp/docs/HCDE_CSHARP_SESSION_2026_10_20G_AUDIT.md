# Session audit 2026-10-20G (chunks 246–261)

## Scope

Whole-file targets: `info.cpp` (246–248), `hcde_edf.cpp` (249–253), `d_dehacked.cpp` (254–256), `g_mapinfo.cpp` (257–260), plus recognition chunk 261 for `IgnoreArmor` VM export.

## Workflow

- Per chunk: managed code commit (or tally-only empty commit where managed code landed earlier), `loc_tally` + matrix commit, push.
- `dotnet build csharp/HCDE.sln -warnaserror` and full `dotnet test csharp/HCDE.sln` after each tally.
- Dedicated recognition tests in `VmActorInfoNativeExportRecognitionTests` (not conversion test pointers).

## Chunk 248 / 261 seam

Initial chunk 248 credited `info.cpp` 697–1043 including the VM export block. Recognition chunk 261 required a gap: chunk 248 now credits 697–914 and 921–1043; chunk 261 credits 915–920 (`recognitionManagedCommit` points at chunk 248 code commit `22adc45abeafe89d468ab96e2445f932e42f67fd`).

## Native LOC this session

| Bucket | Native LOC |
|--------|------------|
| Conversion 246–260 | 5,153 |
| Recognition 261 | 6 |
| **Total** | **5,159** |

## Cumulative (`loc_tally.json`)

| Metric | Value |
|--------|------:|
| Converted behavior native | 33,709 |
| Recognition behavior native | 5,591 |
| Remaining native | 468,786 |

## HEAD

Reported after push: `git rev-parse HEAD` (40 hex chars).
