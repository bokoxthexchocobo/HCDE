# Chunk 103 — `file_zip.cpp` policy

`Zip_FindCentralDir` scan, multipart EOCD rejection, compression-method allowlist, encrypted-entry exclusion; wired into PK3 `AcceptZipEntry`.

Seam: full `FZipFile::Open` directory parse and zip64 extra blocks not ported (SharpCompress handles decode).

Tests: `VmZipResourcePolicyTests`.

## Addendum (re-audit)

Initial tally credited `FZipFile::Open` lines 120–253 (179 native LOC total). Most of that range loads and walks the central directory and applies lump filters inside native code that **SharpCompress** already performs when `VmArchiveScriptLoader` opens PK3 archives. Conversion credit is trimmed to:

- `Zip_FindCentralDir` (47–91)
- Multipart rejection only (140–165)
- Per-entry compression/encryption skip policy (232–253)

**81** native LOC conversion; **88** managed LOC in `VmZipResourcePolicy.cs`. Directory enumeration and inflate/LZMA decode remain a seam.
