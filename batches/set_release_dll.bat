@REM copy release dll (unity) to ksp, quick and official version
echo off

@REM call the local_dev_options
call local_dev_options.bat

call kill_ksp.bat

copy "%KSP2_LOCATION%\backup_dll\UnityPlayer_release.dll" "%KSP2_LOCATION%\UnityPlayer.dll"

