@REM this exemple file should be renamed to ksp_location.bat.
@REM ksp_location.bat is git ignored and therefore can contains informations depending on dev local machine

@REM set your own KSP2_LOCATION and rename me to ksp_location.bat (git ignored)
@REM ps : don't use ""
set KSP2_LOCATION=Z:\SteamLibrary\steamapps\common\Kerbal Space Program 2

@REM Create Zip package (require 7zip)
set Create_zip=True

@REM CLOSE KSP before build
set Close_KSP2=True

@REM Open KSP after build
set Open_KSP2=True

@REM Copy to Other Mods repos
set PostBuildCopy=..\FlightPlan\FlightPlanProject\external_dlls;..\ManeuverNodeController\ManeuverNodeControllerProject\external_dlls
