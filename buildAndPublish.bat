REM Delete current nuget files...
for /r %%x in (*.nupkg) do del "%%x"

REM create new packages
call "build.bat"

REM publish packages
set "_currentPath=%~dp0"
set "_deployPath=%_currentPath%deploy"
call "%_deployPath%\PublishAll.bat"