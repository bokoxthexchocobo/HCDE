# Chunk 1223 — User-command packing, demo-command scan, and IFF chunk lengths from d_protocol.cpp (1-387).

## Scope

User-command packing, demo-command scan, and IFF chunk lengths from d_protocol.cpp (1-387).

## Native ranges

- `src/d_protocol.cpp` **PackUserCmd** 1–387 (387 lines)

**Credited native LOC:** 387 (tally `nativeLoc` 387).

## Managed

`VmProtocol1223Slice.cs`
Managed LOC (tally): 502.
Conversion commit: `87d076c5daab31596b18c3abf418bbcb49a7b8e9`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Serialize (**31–49**) | yes |
| UnpackUserCmd (**51–103**) | yes |
| PackUserCmd (**105–199**) | yes |
| WriteUserCmdMessage (**201–235**) | yes |
| SkipUserCmdMessage (**239–283**) | yes |
| ReadUserCmdMessage (**285–335**) | yes |
| RunPlayerCommands (**337–350**) | yes |
| StartChunk, FinishChunk, SkipChunk (**357–387**) | yes |
| Band hull (**1–387**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A steady nonzero roll that matches the basis becomes DEM_EMPTYUSERCMD. Button bits above bit 28 are not stored in the fourth byte. A skip callback that does not consume bytes ends the scan, and the bytes already read are the stored prefix.

| Managed (`VmProtocol1223Slice.cs`) | Native (`d_protocol.cpp`) |
|------------------------------------|--------------------------|
| Command pack, scan, and IFF lengths | **1–387** |
| Band hull (exclusive slice) | **1–387** |

### Chunk **1223** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **387** |
| **Net headline** | **+387** vs post-**1222** |


**Post-review ratio:** 502/387 managed/native ≈ 130% (src file LOC at HEAD; see tally).

## Tests

- `VmProtocol1223SliceTests.Pack_ResendsHeldAxesAndDropsButtonsAboveBit28`
- `VmProtocol1223SliceTests.Read_StopsWhenSkipStallsAndPadsAnOddChunk`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1223 --to 1223` (0 hard / 0 soft).

## Seams

Integers on the stream are big-endian. Button bytes carry seven bits plus a continuation flag, and the fourth byte stores only bits 21-28, so bits 29-31 survive only when the basis already had them. Pitch, yaw, and the three move axes are written whenever they differ from the basis or are nonzero. Roll is written only when it differs. WriteUserCmdMessage emits an empty command for a steady roll and a full command for a steady nonzero yaw. Unpack into the same address does not copy the basis over itself. Skip and read stop when Net_SkipCommand does not shrink the stream. RunPlayerCommands does nothing when gametic modulo TicDup is nonzero, and it rereads the stored payload on the next tic. FinishChunk with no open chunk returns. SkipChunk of length -1 advances zero bytes. An odd chunk length is padded with one zero.

## Credit rationale

UNCredited d_protocol.cpp 1-387 (+387). The file ends at 387. A full tally line scan shows no overlap.
