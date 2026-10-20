# Chunk 108 addendum — `file_7z.cpp` conversion credit

Reclassified from recognition: `VmSevenZipArchiveReader` implements native `F7ZFile::Open` directory filtering and first-entry extract probe; **SharpCompress** replaces `C7zArchive` / `SzArEx` decode.

Tests: `VmSevenZipArchiveReaderTests`.
