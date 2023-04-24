
# v0.8.0
Features :
* Lift v1 : UI is quite awfull, but it's working. It uses the KSP 2 SAS to turn in a proper direction during ascent burn
  You choose in settings Start Altitude, Wanted Ap and the altitudes where to turn to 45° and 5°. All using ui and sliders.
  Not quite very UI friendly, I should add an ascent profile graph..

  Once Ap is reached burn is stopped and you can finalize the orbit using Circle Tool.
  Not precise enough for the moment it can be improved too. I usually do a second circularize to fully finalize the Final Orbit.

* Drone Flight v1 : Funny way to flight. very expensive in dv ....
  Can Brake agains Surface Speed or in Vertical mode you can choose your V-Speed and fly on Moons.
  Still need improvements to remove horizontal speed, but really precise in playing with Vertical Speed and removing gravity.

Improvements :
* many UI details : main Tabs, added separator, ToolTip position, chapters in settings. Big Work Here.
* Added a progress bar during burn maneuvers.
* Landing v3 : better than in previous version. added a TouchDown Button to force to start the final mode.
  During Touch-Down the speed depends on altitude.
  There is no check of current TWR so, use slow mode for heavy landers.
  I'll adapt to vessel mass and power in future versions.

Fixes :
* Correction of serious lags when Ui is opened (Int Fields and focus trouble)
* Execute Node : added more precision options and warp options. The default values where corrected
* Landing : use sea level if above ground :) and no U Turn facing the sky !
* Circularize : Corrected the double coninc patches.

Know bugs :
* Execute node does not work on multi nodes plans ! meaning if you programmed more than one maneuver the dv computed is totally weird....
* Circularize can sometime create invalid node. please remove nodes and retry.

# v0.7.2
* Landing : correction of the collision detection, more accurate but sometime not perfect, just have a try
* Execute : Added options on direction and start burn time
* UI : added Chapters in settings (more compact)

# v0.7.1 : important corrections
* Rebuild with KSP v0.1.2.0 Assemblies
* Landing : Correction for KSP v0.1.2.0, Landing Touchdown was no more detected.
* Warp : Correction of WarpSpeed ratio.
* Execute : Correction of the OverBurn.

# v0.7.0
* Landing v2  : multi phases landing : Quick Warp, Waiting, Brake and Touch-Down
  after the Brake if the altitude is still too high, another cycle will start.
* Tabs shows if an pilot is currently running
* added Hyperbolic circularize.

# v0.6.0 (not released)
* Landing v1

* Corrected the underBurn in the Execute Node. thanks to @Falki from the KSP2 Modders Society.
* Position of window is saved and cannot be sent outside of Screen space.
* UI new Skin. I'm not really sure of this look. I tried to be closer to main KSP2 UI (purple and black)
* ToolTips to avoid long messages in settings
* Editor Fields in timeWarp Settings avoid sending keys to KSP2 (ex : the 1 Numpad that mute sound)
* Time warp correction : no more burn after time (I hope).

[Known_Issues]
* Circularize can create invalid nodes sometimes, still need to check when and why. (cause KSP2 to have Null Pointer Exceptions)
* During Execute node, if the direction is clearly align at the begining, warp is skipped and burn stats. quite difficult to reproduce 100%.

[TODO] before release

* Land : settings: touch down speed !

[TODO] after and perhaps
* Land : no rotation if speed is up during safe warp
* Land : bad transition between safe warp and quick warp.
* Optimisation : use a cache for settings values
* use ui_shown to Update only needed controllers
* set the node to sub controller on clicking run. (perhaps a bad idea)
* correction circularize UI

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