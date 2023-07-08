@REM copy all assets bundles and manifest
echo off


@REM Open KSP after build
set Src_Bundles_Path=C:\dev\_ksp2\test_Ktools\Assets\Resources
set Dst_Bundles_Path=C:\dev\_ksp2\k2d2\K2D2\assets\bundles

@REM call the local_dev_options
copy %Src_Bundles_Path%\k2d2.bundle %Dst_Bundles_Path%
copy %Src_Bundles_Path%\k2d2.bundle.manifest %Dst_Bundles_Path%




