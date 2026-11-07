# Chunk 1440 — Replicated inventory through presentation-echo append in d_net_diag.cpp (319-668).

## Scope

Replicated inventory through presentation-echo append in d_net_diag.cpp (319-668).

## Native ranges

- `src/d_net_diag.cpp` **HCDEReplicatedInvItem** 319–668 (350 lines)

**Credited native LOC:** 350 (tally `nativeLoc` 350).

## Managed

`VmNetDiag1440Slice.cs`
Managed LOC (tally): 395.
Conversion commit: `6b2929b20b1325823242e7e3b69e6523a3601b72`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Replicated inventory through presentation-echo append (**319–668**) | yes |
| Band hull (**319–668**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Presentation-echo reader | lines 671-791 stay outside this span |
| Echo reset and dump | already credited at 797-812 and 813-1075 |
| Struct, comments, and the blank line | non-behavior inside **319–668** |

A non-Hexen item leaves the five slots at zero. A Hexen armor item clamps each slot into 0 through 65535. Ammo replicates with a flag byte of zero. A null item has no flags. Armor amounts above the signed maximum clamp. Hexen armor copies slots and leaves the amount alone. The authority does not rewrite its own inventory. A slot other than the console player is ignored. A missing inventory class is given, then every amount is pinned. A class that is not inventory is skipped, and extras already on the client stay. The echo magic is ECHO and the version is 8. The writer's own player record is sent only when the weapon change flags are nonzero, and that send remembers the snapshot. A reserved slot writes 255 and no items. Debug mode sends the requested player list with both change bits, and a player number of 64 or more fails after the header without recording a lane. A short buffer does not rewind the cursor.

| Managed (`VmNetDiag1440Slice.cs`) | Native (`d_net_diag.cpp`) |
|-----------------------------------|---------------------------|
| Inventory reconcile through echo append | **319–668** |
| Band hull (exclusive slice) | **319–668** |

### Chunk **1440** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **350** |
| **Net headline** | **+350** vs the prior chunk |


**Post-review ratio:** 395/350 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmNetDiag1440SliceTests.SlotsFlagsAndReconcile`
- `VmNetDiag1440SliceTests.AppendEchoWire`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1440 --to 1440` (0 hard / 0 soft).

## Seams

Echo-string append, weapon-change flags, and the lane record are callees. The reader starts at line 671.

## Credit rationale

UNCredited d_net_diag.cpp 319-668 (+350). One contiguous span. The struct, comments, and blank line inside it are non-behavior. The span does not cross credited code.
