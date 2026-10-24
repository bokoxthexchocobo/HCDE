# Chunk 144 — mixed UTF-8 / Win-1252 + response `Uncomment`

`VmUtf8MixedEncoding` (`GetCharFromString`, `MakeUtf8` subset) and `VmMiscResponseUncomment`.

**Not credited:** full `MakeUTF8` static buffer, `M_FindResponseFile` file IO.

Tests: `VmUtf8MixedEncodingTests`, `VmMiscResponseUncommentTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
