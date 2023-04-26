@REM used to get all dlls from skp 2 and unity. saved to externals_dll directory

@REM call the ksp_location
call local_dev_options.bat

@REM echo loc:%KSP2_LOCATION%
echo off

set OUTPUT=output
set DEST_DIR=..\external_dlls

echo KSP_LOC : %KSP2_LOCATION%
echo Dest_Dir is : %DEST_DIR%

echo create local dir %DEST_DIR%
if not exist %DEST_DIR% mkdir %DEST_DIR%

echo ####################### Get assembly dll #######################
copy %KSP2_LOCATION%\KSP2_x64_Data\Managed\Assembly-CSharp.dll %DEST_DIR%

copy %KSP2_LOCATION%\BepInEx\core\BepInEx.dll %DEST_DIR%
copy %KSP2_LOCATION%\BepInEx\plugins\SpaceWarp\SpaceWarp.dll %DEST_DIR%

copy %KSP2_LOCATION%\KSP2_x64_Data\Managed\Newtonsoft.Json.dll %DEST_DIR%

copy %KSP2_LOCATION%\KSP2_x64_Data\Managed\UnityEngine.TextRenderingModule.dll %DEST_DIR%
copy %KSP2_LOCATION%\KSP2_x64_Data\Managed\UnityEngine.InputLegacyModule.dll %DEST_DIR%
copy %KSP2_LOCATION%\KSP2_x64_Data\Managed\UnityEngine.IMGUIModule.dll %DEST_DIR%
copy %KSP2_LOCATION%\KSP2_x64_Data\Managed\UnityEngine.CoreModule.dll %DEST_DIR%
copy %KSP2_LOCATION%\KSP2_x64_Data\Managed\UnityEngine.dll %DEST_DIR%


