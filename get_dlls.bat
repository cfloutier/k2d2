@REM used to get all dlls from skp 2 and unity. saved to externals_dll directory


echo off
set PROJECT_NAME=AutoExecuteNode

set OUTPUT=output
set DEST_DIR=.\%PROJECT_NAME%\external_dlls

echo Dest_Dir is : %DEST_DIR%

echo create local dir %DEST_DIR%
if not exist %DEST_DIR% mkdir %DEST_DIR%

echo ####################### Get assembly dll #######################
set KSP_PATH="D:\SteamLibrary\steamapps\common\Kerbal Space Program 2"

copy %KSP_PATH%\KSP2_x64_Data\Managed\Assembly-CSharp.dll %DEST_DIR%
copy %KSP_PATH%\BepInEx\core\BepInEx.dll %DEST_DIR%
copy %KSP_PATH%\KSP2_x64_Data\Managed\Newtonsoft.Json.dll %DEST_DIR%
copy %KSP_PATH%\BepInEx\plugins\SpaceWarp\SpaceWarp.dll %DEST_DIR%
copy %KSP_PATH%\KSP2_x64_Data\Managed\UnityEngine.TextRenderingModule.dll %DEST_DIR%
copy %KSP_PATH%\KSP2_x64_Data\Managed\UnityEngine.InputLegacyModule.dll %DEST_DIR%
copy %KSP_PATH%\KSP2_x64_Data\Managed\UnityEngine.IMGUIModule.dll %DEST_DIR%
copy %KSP_PATH%\KSP2_x64_Data\Managed\UnityEngine.CoreModule.dll %DEST_DIR%
copy %KSP_PATH%\KSP2_x64_Data\Managed\UnityEngine.dll %DEST_DIR%


