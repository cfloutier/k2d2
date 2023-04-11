
# v0.7.0
* Landing v2 (2 phases landing )

# v0.6.0 (not released)
* Landing v1

* [Execute_NODE] Corrected the underBurn in the Execute Node. thanks to @Falki from the KSP2 Modders Society. 
* Position of window is saved and cannot be sent outside of Screen space.
* UI new Skin. I'm not really sure of this look. I tried to be closer to main KSP2 UI (purple and black)
* ToolTips to avoid long messages in settings
* Editor Fields in timeWarp Settings avoid sending keys to KSP2 (ex : the 1 Numpad that mute sound)
* Time warp correction : no more overload (I hope). We also can use the Standard KSP2 timewarp (in settings) but there are trouble at the end of phase...

[Known_Issues]
* Circularize can create invalid nodes. (cause KSP2 to have Null Pointer Exceptions)
* During Execute node, if the direction is clearly align at the begining, warp is skipped and burn stats 


[TODO] before release
setting back
cleanup maneuvre debug info

* main tabs : shows if a pilot is running
* 
* Land : new messages
* Land : no rotation if speed is up during safe warp
* bad transition between safe warp and quick warp. 

* correction circularize UI

* correct tooltip timing

[TODO] after
* set the node to sub controller on clicking run.



# v0.5.1
* Rebuild with SpaceWarp 1.0.1 + fix the version number and KSP2 version check

# v0.5.0
* Added Circularize !!! thanks to Mole

# v0.4.0
* Burn dv is computed with quite good accuracy.
* [AutoManeuver] Use dV rather than time to compute burning durtion a thrust ratio.

# v0.3.0
* [internal] added new classes to load and save settings easily.
* Added Settings
* Turn phase : started the sas to maneuvre mode in a proper way
* Time Warp Phase : Added Settings and a more precise safe duration before burn

# v0.2.1
* Rebuild for ksp2 v0.1.1

# v0.2.0
* added Ugly icon in the app bar and title bar
* Renamed to K2-D2

# v0.1.0

* Very first implementation.