# thingdef_parse.cpp ParsePropertyParams — chunk 21 audit

## Scope

Port native `ParsePropertyParams` parameter decoding (`I`, `F`, `S`, `T`, `C`, `M`, `N`, `L`, `Z`, `X`) without the ZScript expression compiler or property handler dispatch.

**Native:** `ParsePropertyParams` lines 655–845 (191 lines).

**Managed:** `DecoratePropertyParamParser.cs` (246 lines) + `DecoratePropertyParamParserTests`.

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
