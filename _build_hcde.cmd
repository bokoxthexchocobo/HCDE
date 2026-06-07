@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
cmake --build build --config RelWithDebInfo --target zdoom
if %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%
cmake --build build --config RelWithDebInfo --target hcdercon
if %ERRORLEVEL% NEQ 0 exit /b %ERRORLEVEL%
copy /y build\hcde.exe build\RelWithDebInfo\hcde.exe
copy /y build\hcdeserv.exe build\RelWithDebInfo\hcdeserv.exe
copy /y build\hcdercon.exe build\RelWithDebInfo\hcdercon.exe
