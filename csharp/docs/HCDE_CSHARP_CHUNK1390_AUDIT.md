# Chunk 1390 — The DDS fourcc and flag constants and the texture class declaration from ddstexture.cpp (1-186).

## Scope

The DDS fourcc and flag constants and the texture class declaration from ddstexture.cpp (1-186).

## Native ranges

- `src/common/textures/formats/ddstexture.cpp` **FDDSTexture** 1–186 (186 lines)

**Credited native LOC:** 186 (tally `nativeLoc` 186).

## Managed

`VmDdsTexture1390Slice.cs`
Managed LOC (tally): 278.
Conversion commit: `cddb5d0bf64ce41451d4e2bd0428bd6c21ef5fa7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Fourcc ids and flag bits (**51–89**) | yes |
| Pixel format, caps, surface, and file header (**97–136**) | yes |
| Texture class declaration (**145–178**) | yes |
| Band hull (**1–186**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CheckDDS | starts at line 187, outside this hole |
| Constructor and pixel reads | method bodies after the class declaration |

Little-endian MAKE_ID packs DDS and DXT1 through DXT5. The cubemap negative-Z flag keeps the native spelling NEGATIZEZ. DDSD_PIXELFORMAT, DDSCAPS_TEXTURE, and DDSCAPS2_CUBEMAP_POSITIVEY share the value 0x1000 because they live in different flag words. The surface descriptor is 124 bytes, the pixel format is 32, the caps block is 16, and the file header is 128. Pitch and LinearSize share one 32-bit slot. Reserved1 has 11 words. A surface Size field may hold the DDS fourcc; this band does not force it to 124. The class extends FImageSource, friends FTexture, and declares pixel modes 0, 1, and 2. CreatePalettedPixels and CopyPixels default frame to 0. DecompressDXT3 and DecompressDXT5 take premultiplied. DecompressDXT1 and ReadRGB do not. CalcBitShift is a protected static declaration. CheckDDS is not a declared method.

| Managed (`VmDdsTexture1390Slice.cs`) | Native (`ddstexture.cpp`) |
|--------------------------------------|---------------------------|
| Fourcc, flags, header layout, and the class declaration | **1–186** |
| Band hull (exclusive slice) | **1–186** |

### Chunk **1390** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **186** |
| **Net headline** | **+186** vs the prior chunk |


**Post-review ratio:** 278/186 managed/native ≈ 149% (src file LOC at HEAD; see tally).

## Tests

- `VmDdsTexture1390SliceTests.FourCcFlagsAndHeaderLayout`
- `VmDdsTexture1390SliceTests.ClassDeclaresModesFieldsAndMethods`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1390 --to 1390` (0 hard / 0 soft).

## Seams

CheckDDS starts at line 187 and stays outside this band. The constructor body, bit-shift helper, and pixel reads stay after the class declaration.

## Credit rationale

UNCredited ddstexture.cpp 1-186 (+186). After chunk 1389 this is the unique largest hole. c_bind.cpp 1-182 is 182. The band is shorter than 400 lines and is not sliced.
