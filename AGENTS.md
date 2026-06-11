## Cursor Cloud specific instructions

- HCDE is a native CMake/C++ project; the top-level README and `wiki/Building.md` remain the source of truth for standard build commands.
- In this Cursor Cloud image, configure development builds with GCC explicitly, for example `CC=gcc CXX=g++ cmake -S . -B build-dev -G Ninja -DCMAKE_BUILD_TYPE=Debug -DFORCE_NO_LTO=ON -DENABLE_IWYU=OFF`. The default `/usr/bin/c++` resolves to Clang here and cannot link `libstdc++`.
- Full gameplay and RCON validation for `hcde`/`hcdeserv` requires a valid IWAD such as `DOOM2.WAD`; the Python harness docs under `tests/` describe the runtime arguments. `hcdemaster` can be smoke-tested without an IWAD by sending a local heartbeat and launcher-list query.
- A default top-level configure currently does not register CTest tests; use the repository's Python validation harnesses and CMake target builds for practical checks.
