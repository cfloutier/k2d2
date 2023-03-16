Infos for building the Mod

-----------------

# get dll

Assembly are localy copied using the get_dlls.bat
the .csproj use relative link to each dll

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








