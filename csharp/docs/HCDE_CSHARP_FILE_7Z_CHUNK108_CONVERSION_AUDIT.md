# Chunk 108 addendum — `file_7z.cpp` credit split

`VmSevenZipArchiveReader` landed in `b051701e` (53 LOC). This PR only changed `internal` → `public` and added `VmSevenZipArchiveReaderTests`; **no new managed decode logic**.

| Bucket | Native range | Managed | Notes |
|--------|----------------|---------|--------|
| **Recognition** | `F7ZFile::Open` 236–280 | `VmSevenZipArchiveReader` (pre-existing) | Directory skip + first-entry extract probe mirrored in `GetEntries`. |
| **Not credited** | 197–225 | — | `C7zArchive::Open` / `SzArEx` — **SharpCompress** only. |
| **Conversion** | — | **0** | Public visibility + tests are not behavior conversion LOC. |

Tests: `VmSevenZipArchiveReaderTests` (parity evidence for recognition).
