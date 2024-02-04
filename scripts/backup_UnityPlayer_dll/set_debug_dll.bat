@REM copy debug dll (unity) to ksp, useful for debuging
echo off

@REM call the local_dev_options
call ..\local_dev_options.bat

@REM call ..\kill_ksp.bat

copy "UnityPlayer_debug.dll" "%KSP2_LOCATION%\UnityPlayer.dll"

