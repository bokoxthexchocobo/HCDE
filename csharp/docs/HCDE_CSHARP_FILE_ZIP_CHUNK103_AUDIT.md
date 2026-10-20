# Chunk 103 — `file_zip.cpp` policy

`Zip_FindCentralDir` scan, multipart EOCD rejection, compression-method allowlist, encrypted-entry exclusion; wired into PK3 `AcceptZipEntry`.

Seam: full `FZipFile::Open` directory parse and zip64 extra blocks not ported (SharpCompress handles decode).

Tests: `VmZipResourcePolicyTests`.
