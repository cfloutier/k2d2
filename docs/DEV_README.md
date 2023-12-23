How to build and debug this Mod
-----------------------------
# Build 
## 01 : configure your KSP 2 path

Copy the `batches\local_dev_options.exemple.bat` to `batches\local_dev_options.bat`

The `batches\local_dev_options.bat` is git ignored and can contains local dev information

```bat
@REM this exemple file should be renamed to ksp_location.bat.
@REM ksp_location.bat is git ignored and therefore can contains informations depending on dev local machine

@REM set your own KSP2_LOCATION and rename me to ksp_location.bat (git ignored)
@REM ps : don't use ""
set KSP2_LOCATION=Z:\SteamLibrary\steamapps\common\Kerbal Space Program 2

@REM CLOSE KSP before build
set Close_KSP2=True
@REM Open KSP after build
set Open_KSP2=True
```

I've installed on my laptop KSP2 in the Z: Drive...
adapt it to your local configuration.

## 02 : get dependencies dll

Once the KSP path is defined go to the `batches` folder and run `get_dlls.bat`
it assumes that SpaceWarp is already installed in your KSP2 folder
Assembly are localy copied using to the `external_dlls` folder, created if not found
the .csproj use relative link to each dll

The list of getted dll is adpated to what the K2D2 mod needs.  

## 03 : Build

The Build use a single post process intruction that call the batches/post_build.bat

This will call
* make_zip.bat - it prepares the release package with proper version   
* copy_to_ksp.bat - this will copy the dll and all dependencies to the proper ksp2 folder (`Kerbal Space Program 2\BepInEx\plugins\K2D2`)

This work fines under VisualStudio 2022 and VSCode. (use Ctrl+Shift+B for VSCode) 
VisualStudio is quicker then VsCode but I loke VSCode because the the whole project is freely editable without the useless "solution" stuff that hide too many things.  

## Versions

`swinfo.json` is where all information of the Mod is used by SpaceWarp Mod (the main mod loader)

see : https://spacedock.info/mod/3277/Space%20Warp%20+%20BepInEx

the version is only set in the .csproj file : `K2D2Project\K2D2.csproj`

during build the swinfo.json is modified to get the version number

## .Net version

the .net framework is Standard 2.1. Thanks to @schlosrat that hep me having a simple csproj


# Debug

To debug I use the tutorial : https://gist.github.com/gotmachine/d973adcb9ae413386291170fa346d043

I'd like to write an addapted version of this here









