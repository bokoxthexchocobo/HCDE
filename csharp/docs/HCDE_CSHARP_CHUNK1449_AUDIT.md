# Chunk 1449 — Live wrap helpers and gameplay envelope prefix in d_net.cpp (2628-2807).

## Scope

Live wrap helpers and gameplay envelope prefix in d_net.cpp (2628-2807).

## Native ranges

- `src/d_net.cpp` **ShouldWrapHCDEClientCommandPacket** 2628–2807 (180 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmNet1449Slice.cs`
Managed LOC (tally): 220.
Conversion commit: `7eea57122d25c8826de38fac8aa34bc5de5a07ed`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Wrap and envelope prefix (**2628–2807**) | yes |
| Band hull (**2628–2807**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Live replay | lines 2418-2626 are already credited |
| Room staleness gate | lines 2809-2813 are already credited |

Dedicated input is authorized only when the packet names a single player and that player is the sender. A non-player authority keeps the player settings bit clear. The envelope writer clears the outgoing flag byte after copying it. A version, kind, or flag mismatch returns before the room check, and a matching prefix does not reject a different room.

| Managed (`VmNet1449Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| WrapClient through UnwrapPrefix | **2628–2807** |
| Band hull (exclusive slice) | **2628–2807** |

### Chunk **1449** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **180** |
| **Net headline** | **+180** vs the prior chunk |


**Post-review ratio:** 220/180 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1449SliceTests.WrapAuthorityAndSettings`
- `VmNet1449SliceTests.EnvelopePrefixStopsBeforeRoom`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1449 --to 1449` (0 hard / 0 soft).

## Seams

ShouldWrapHCDEClientCommandPacket through the version check in UnwrapHCDEGameplayEnvelope. RequestHCDELiveReplay ends at 2626. The room-staleness gate at 2809-2813 is already credited.

## Credit rationale

UNCredited d_net.cpp 2628-2807 (+180). One contiguous span of the wrap predicates, authority settings, and the envelope prefix. The span stops on the closing brace before the credited room check and does not cross credited code.
