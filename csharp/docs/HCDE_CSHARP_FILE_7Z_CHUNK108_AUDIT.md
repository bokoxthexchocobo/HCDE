# Chunk 108 — `file_7z.cpp` (recognition)

`VmSevenZipArchiveReader` already mirrors native `F7ZFile::Open` directory filtering and first-entry extract validation; decode is delegated to SharpCompress (`SevenZipArchive.OpenArchive`).

Tests: `VmSevenZipArchiveReaderTests`.
