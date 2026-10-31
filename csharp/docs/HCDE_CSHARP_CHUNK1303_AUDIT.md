# Chunk 1303 — Texture upload, the unpack buffer, and the sampler bind from gl_hwtexture.cpp (1-331).

## Scope

Texture upload, the unpack buffer, and the sampler bind from gl_hwtexture.cpp (1-331).

## Native ranges

- `src/common/rendering/gl/gl_hwtexture.cpp` **FHardwareTexture::CreateTexture** 1–331 (331 lines)

**Credited native LOC:** 331 (tally `nativeLoc` 331).

## Managed

`VmGlHwTexture1303Slice.cs`
Managed LOC (tally): 345.
Conversion commit: `4cbdaa606a1ae367c2dd541852cb22143bcda84d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Filter table and last-bound slots (**41–56**) | yes |
| CreateTexture (**68–161**) | yes |
| AllocateBuffer and MapBuffer (**169–190**) | yes |
| Destructor, bind, and unbind (**197–246**) | yes |
| Depth buffer and framebuffer (**254–281**) | yes |
| BindOrCreate (**290–329**) | yes |
| Band hull (**1–331**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Max texture size and the filter cvar | host driver and cvar |
| Debug object label | recorded only when the debug API is on and the level is not zero |
| Box downsample body | callee invoked from the scale path; this band owns the call |

The filter table is seven rows. Row 6, the cvar default, is linear mipmapped minification, nearest magnification, and mipmaps on. Row 2 is linear with mipmaps off. CreateTexture generates an id on the first call. Unit -1 reads the current 2D binding and restores it after the upload, and it does not write the last-bound slot. A positive unit activates that unit and activates unit 0 again at the end. Unit 0 only binds. A debug label is emitted only when the debug API is present and the level is not zero. A live unpack buffer is unmapped and the CPU pointer is discarded, so the upload reads the buffer. A null pointer with no unpack buffer allocates four times width times height-plus-one zero bytes, clears the mip flag, and frees that block after the upload. A source larger than the cap in either axis is box-downsampled into the same padded size and freed. Four channel bytes keep RGBA8 and BGRA. One, two, or three bytes select R8/RG8/RGB8 and red/RG/BGR, and they set unpack alignment to 1. Zero channel bytes keep BGRA and RGBA8. The first upload, and every upload without an unpack buffer, is TexImage. A later upload with an unpack buffer is TexSubImage, then alignment 4 and the unpack buffer is unbound. Mipmaps are generated only when the caller asked and the selected filter row has them. AllocateBuffer forces a texel size outside 1 through 4 back to 4, stores the source width as the pitch, and creates the unpack buffer when at least one axis still fits the cap. Both axes over the cap leave the id at zero. Mapping that missing buffer binds zero and returns null. The destructor deletes a texture id and a buffer id once. Bind returns immediately when the unit's last id matches, which also skips a late mipmap request. Otherwise it binds, generates mipmaps when they were missing, and a non-zero unit is bracketed by active-texture calls. Unbind clears a non-zero slot and binds texture zero. Unbind-all walks units 0 through 15. The depth renderbuffer is created once at the capped size with depth-stencil storage. The framebuffer attach uses the color texture and calls the depth getter for both the depth and the stencil attachments. BindOrCreate wants mipmaps when the clamp mode is XY or below and filtering is allowed. A miss with the indexed flag forces one byte, disables filtering, and drops the mipmap request. A non-canvas source records the translation and the flags with the process-data bit before the upload. A canvas passes a null pointer and its own size, so an 8 by 2 image under a cap of 4 uploads as 4 by 2. The indexed canvas still allocates the four-byte padded block and uploads it as R8. After the bind, a forced no-filter mode adds the no-filter offset when the clamp mode is still XY or below, and XY-no-mip is left alone. The sampler is bound with light 255. A cached bind does not upload again and still adjusts the clamp.

| Managed (`VmGlHwTexture1303Slice.cs`) | Native (`gl_hwtexture.cpp`) |
|--------------------------------------|----------------------------|
| Upload, unpack buffer, and sampler bind | **1–331** |
| Band hull (exclusive slice) | **1–331** |

### Chunk **1303** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **331** |
| **Net headline** | **+331** vs post-**1302** |


**Post-review ratio:** 345/331 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmGlHwTexture1303SliceTests.Upload_ScalesPastTheCapAndUploadsThroughTheUnpackBuffer`
- `VmGlHwTexture1303SliceTests.Bind_CachesTheUnitAndOffsetsTheIndexedClamp`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1303 --to 1303` (0 hard / 0 soft).

## Seams

The max texture size, the filter cvar, and the debug-label gate are host state. The box downsample is the callee used when the source is larger than that cap.

## Credit rationale

UNCredited gl_hwtexture.cpp 1-331 (+331). A full tally line scan shows no overlap. The file ends at line 331.
