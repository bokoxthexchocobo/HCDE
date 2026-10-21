# Chunk 644 — NMS1 register/unregister/heartbeat orchestration (`sv_master.cpp` 466–672)

## Scope

NMS1 register/unregister/heartbeat orchestration (`sv_master.cpp` 466–672)

## Native ranges

- `src/common/engine/sv_master.cpp` **TryRegisterNms1Master** 466–672 (207 lines)

**Credited native LOC:** 207 (tally `nativeLoc` 207).

## Managed

`VmSvMasterNms1Lifecycle.cs`
Managed LOC (tally): 104.
Conversion commit: `e9bd56f6ce3b26202a19698b23f9d33b280ef338`.

## Padding review (2026-10-21)

| Managed | Native (`sv_master.cpp`) |
|---------|---------------------------|
| `ShouldSkipRegisterAttemptNative` | 503–508 |
| `ShouldReRegisterOnPortChangeNative` | 623–627 |
| `CanHeartbeatNative` / `ResolveHeartbeatGamePortNative` | 618–635 |

**Post-review ratio:** 104/207 managed/native ≈ 50% (src file LOC at HEAD; see tally).

## Tests

- `VmSvMasterNms1LifecycleTests.ShouldReRegisterOnPortChangeNative_WhenStoredPortDiffers`
- `VmSvMasterNms1LifecycleTests.ShouldSkipRegisterAttemptNative_RespectsCooldown`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 644 --to 644` (0 hard / 0 soft).

## Seams

`I_GetLocalServerSnapshot`, packet writes, and socket sends are host callbacks.

## Credit rationale

Whole `TryRegisterNms1Master`, `TryUnregisterNms1Master`, and `TryHeartbeatNms1Master`.
