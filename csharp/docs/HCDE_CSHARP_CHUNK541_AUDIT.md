# Chunk 541 — HCDE DEM naming, event-record conversion loop, canonical string + legacy weapon/CVar append

## Scope

HCDE DEM naming, event-record conversion loop, canonical string + legacy weapon/CVar append

## Native ranges

- `src/d_net.cpp` **HCDEDemCommandName** 3902–3923 (22 lines)
- `src/d_net.cpp` **HCDEAppendEventRecords** 3932–4026 (95 lines)
- `src/d_net.cpp` **HCDEReadCanonicalString** 4028–4051 (24 lines)
- `src/d_net.cpp` **HCDEAppendLegacyWeaponIndex** 4062–4072 (11 lines)
- `src/d_net.cpp` **HCDEAppendLegacyCVarChange** 4074–4109 (36 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmDNetAppendEventRecords.cs`
Managed LOC (tally): 234.
Conversion commit: `c30d0b6e296c8627cbdd1db4b594b7192a0a4a7c`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `HCDEDemCommandName` **3902–3923**, `HCDEAppendEventRecords` **3932–4026**, `HCDEReadCanonicalString` **4028–4051**, legacy weapon/CVar append **4062–4109** (`d_net.cpp`).

| Managed | Native (`d_net.cpp`) |
|---------|----------------------|
| `DemCommandName` | `HCDEDemCommandName` **3902–3923** |
| `ValidateAppendPreconditions` / `AppendEventRecords` / `EventCountOverflow` / `ShouldDropClientInputEvent` / `PayloadTooLarge` / `WriteEventCountBE16` / `WritePayloadSizeBE16` | `HCDEAppendEventRecords` **3932–4026** |
| `TryReadCanonicalString` | **4028–4051** |
| `TryAppendLegacyWeaponIndex` | **4062–4072** |
| `TryAppendLegacyCVarChange` / `LegacyCVarHeaderByte` | **4074–4109** |
| `TryReadByte` / `TryAppendOutputByte` / `SinChangedXorEvent` | helper seams inside **3932–4026** |

### Out-of-body uncredit (541)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **188** |
| **Out-of-body removed** | **0** |
| **Honest** | **188** |


**Post-review ratio:** 234/188 managed/native ≈ 124% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetAppendEventRecordsTests.DemCommandName_KnownPause`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_ClientInputFilterRewinds`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_RejectsDisallowedTicEvent`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_RejectsCursorAtCapacity`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_WritesTwoEventPayloadSizes`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_RejectsPayloadTooLarge`
- `VmDNetAppendEventRecordsTests.TryReadCanonicalString_RoundTrip`
- `VmDNetAppendEventRecordsTests.TryAppendLegacyWeaponIndex_TwoByteForm`
- `VmDNetAppendEventRecordsTests.TryAppendLegacyCVarChange_BoolPayload`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 541 --to 541` (0 hard / 0 soft).

## Seams

`HCDEAppendCanonicalEventPayload`, `HCDEReadByteField`, `HCDEAppendByte`, and live `HCDELiveWriteBE16` buffer mutation are injected at seams; debug trace logging is not hosted.

## Credit rationale

Credits whole `HCDEDemCommandName`, `HCDEAppendEventRecords`, `HCDEReadCanonicalString`, `HCDEAppendLegacyWeaponIndex`, and `HCDEAppendLegacyCVarChange` bodies mirrored in managed code (not helper-only extractions).
