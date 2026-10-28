# Chunk 131 — `utf8.cpp` encode/decode

`VmUtf8Codec` mirrors `utf8_encode` and `utf8_decode` (first-tier BMP/surrogate rules).

Tests: `VmUtf8CodecTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
