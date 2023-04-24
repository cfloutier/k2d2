@echo off


taskkill /f /im KSP2_x64.exe
timeout 2


dotnet build k2d2.sln -c Debug
call make_zip.bat Debug
call copy_to_ksp.bat