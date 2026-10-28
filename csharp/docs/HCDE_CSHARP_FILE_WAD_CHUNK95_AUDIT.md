# Chunk 95 — `file_wad.cpp` (Open namespace path)

## Faithful

- Big-endian WAD header fallback, directory decode, LZSS implicit compressed size, `IsMarker`, `SetNamespace` (flat hack, sprite size gate, overlap warnings), `SkinHack`, `TryOpenResource` wiring.

## Seams

- No `GenerateHash`, `FResourceFile` allocation, or `CheckWad` factory.
- Lump names keep WAD ASCII casing (native `tolower` on all name bytes); markers matched case-insensitively.
- `IWadArchiveMessageSink` replaces `FileSystemMessageFunc`.

## Tests

- `WadArchiveNamespaceTests`, `TestWadBuilder.BuildCustomWad`.
