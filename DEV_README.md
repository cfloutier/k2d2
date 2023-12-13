Infos for building the Mod

-----------------

# 01 : configure your KSP 2 path

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

I've installed KSP in the Z: Drive...

# 02 : get dependencies dll

once the KSP path is defined go to the `batches` folder and run `get_dlls.bat`
it assumes that SpaceWarp is already installed in your KSP2 folder
Assembly are localy copied using to the `external_dlls` folder, created if not found
the .csproj use relative link to each dll

# 03 : Build

the Build use a single post process intruction that call the batches/post_build.bat

this will call the make_zip.bat, it prepares the release version in the output/be creating a zip at the root of the 

# Build

I build using VisualStudio 2022. But It could be fine with older version

the .net used is 4.8 because LazyOrbit used that too. I don't really know if this is a good thing.
the 4.7.X is the one used by ksp 2.

for the moment I can run and debug with 4.8 without trouble. We could use another version of .net I needed. I not stuck to 4.8 at all

the csproj use the PostBuildEvent to build the zip and copy it to ksp dir. see
* make_zip.bat
* copy_to_ksp.bat

(some pathes are hard coded , we should find a way to definite it a better way, perhaps by using env variables ?)

I only build in debug for the moment but there were no trouble usign release version in ksp2.

------------------

I also use VsCode because I find it easier to use for editing. by the disassembly is less verbose, we don't have the content of functions.

Build in vscode works perfectly but I could have been able to debug properly for the moment.
and the build is quicker in VisualStudio. therefore, for the moment I use Both

# Debug

To debug I use the tutorial : https://gist.github.com/gotmachine/d973adcb9ae413386291170fa346d043
dll of ksp have to be modified,
it only working in VisualStudio for the moment








