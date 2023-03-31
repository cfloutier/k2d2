@REM copy debug dll (unity) to ksp

@REM call the ksp_location
call ksp_location.bat

taskkill /f /im KSP2_x64.exe
timeout 2

copy %KSP2_LOCATION%\backup_dll\UnityPlayer_debug.dll %KSP2_LOCATION%\UnityPlayer.dll

