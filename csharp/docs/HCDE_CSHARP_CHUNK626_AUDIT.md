# Chunk 626 — Retired: split `GetPacket` credit merged into chunk625 (2119–2335). Former 2126–2320 range split mid-function at recvfrom.

## Scope

Retired: split `GetPacket` credit merged into chunk625 (2119–2335). Former 2126–2320 range split mid-function at recvfrom.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`i_net.cpp`) |
|---------|-------------------|
| _(retired)_ | merged into `GetPacketNative` chunk625 |

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 626 --to 626` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
