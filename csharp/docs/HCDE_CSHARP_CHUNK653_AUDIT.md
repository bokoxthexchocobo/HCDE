# Chunk 653 — Whole `HCDEAppendLegacyRunArgs` and `HCDEAppendLegacyEventPayload` (`d_net.cpp` 4111–4299, `.inl` tail excluded).

## Scope

Whole `HCDEAppendLegacyRunArgs` and `HCDEAppendLegacyEventPayload` (`d_net.cpp` 4111–4299, `.inl` tail excluded).

## Native ranges

- `src/d_net.cpp` **HCDEAppendLegacyRunArgs** 4111–4135 (25 lines)
- `src/d_net.cpp` **HCDEAppendLegacyEventPayload** 4137–4299 (163 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmDNetLegacyEventPayload.cs`, `VmDNetLegacyEventPayloadAppend.cs`
Managed LOC (tally): 356.
Conversion commit: `4b76b7b4a75b372857afa3b297711db66b24f1b9`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|----------------------|
| `AppendLegacyRunArgsNative` | `HCDEAppendLegacyRunArgs` 4111–4135 |
| `AppendLegacyEventPayloadNative` | `HCDEAppendLegacyEventPayload` 4137–4299 |
| `IsZeroPayloadEvent` / trailing-byte helpers | same switch cases (zero-payload and field sizes) |
| `TryPlanLegacyRunArgs` / `MaskRunArgCount` | `HCDEAppendLegacyRunArgs` named branch 4114–4122 |
| `TryReadSetSlotHeader` / `SetSlotNeedsPnumPrefix` | `DEM_SETSLOT` / `DEM_SETSLOTPNUM` 4275–4298 |


**Post-review ratio:** 356/188 managed/native ≈ 189% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetLegacyEventPayloadAppendTests.AppendLegacyRunArgsNative_NamedMasksHighBit`
- `VmDNetLegacyEventPayloadTests.TryPlanLegacyRunArgs_NamedMasksCount`
- `VmDNetLegacyEventPayloadAppendTests.AppendLegacyEventPayloadNative_PauseZeroPayload`
- `VmDNetLegacyEventPayloadAppendTests.AppendLegacyEventPayloadNative_WarpCheatFields`
- `VmDNetLegacyEventPayloadTests.IsZeroPayloadEvent_Pause`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 653 --to 653` (0 hard / 0 soft).

## Seams

`.inl` continuation, live `HCDEAppendLegacyStringFromCanonical`, and `HCDEAppendLegacyCVarChange` bodies are injected on the sink.

## Credit rationale

Full switch port in `VmDNetLegacyEventPayloadAppend`; sizing helpers in `VmDNetLegacyEventPayload` mirror the same cases for tests.
