# v0.11.3

* AutoStaging is off by default
* fixed an issue when switching to Tracking Station (errors in logs, thanks to @munix)
* fine adjust Ap during ascent profile (Lift-Off)

# v0.11.2

* Correction of the Staging tool : working with asparagus models. (I hope)
* Docking pilot is back without the A.R
* Time Warp is now allowed during node burn execution. 
K2D2 is just following the same rules as "Warp Before Node execution". A Max time warp is computed and applied. 
* fixed an issue when switching to Space Center (errors in logs, thanks to @munix)
* Warp Tabs has been removed (needed to be fixed)

# v0.11.1

* Rebuild with KSP v0.2
* Added a resize option using Matrix trick. It can be blurry but it's a quick solution for large screen.
* Added Autostaging tool ! 
  The little S button in the toolbar can switch it on/off quickly.
  The Stagin tab is only used for information on next stage. I'll try to add a little timer.
  Will Active Next Stage when at least one tank is empty in the current stage.
  Added a delay to avoid rotation during Lift pilot. 1-2 seconds are good values. 
  It can avoid a crash if a tank hit the vessel during staging.
  AutoStage is a global feature, meaning it is active even if no K2D2 pilot is running. Use it with causion then
* Dock has been removed because of rendering issues of the "Augmented Reallity" lines

# v0.10.1
* fixes by @schlosrat to match patch kerbal 2 v1.5.0 (thanks)
# v0.10.0
Docking Assistant :
1. added like "Duna Colonist's HUD" arrows and line to 
  * see all docks part and help choosing the right one (in your vessel and in target)
  * see your speed !
  * see the align dock position
2. added a tool to kill the speed using the main thrust. It is usefull for the last kilometers
3. added a tool to kill speed using rcs : usefull furing the final approach
4. added a full auto docking pilot. This one really need further work but it's quite working using my small vessel. 

Fix :
* fixed the soi change time in the Warp to Pilot

# v0.9.4
Fix :
* rebuild with KSP2 v1.4.0

# v0.9.3
Fix :
* rebuild with KSP2 v1.3.1

Features:
* Lift : added a full curve rendering of the ascent profile.

Blue color represent the atmosphere height and white the atmosphere density. Green line is the current vessel's altitude, greys are controled altitudes and cyan the end of atm

# v0.9.2
Fixes : typos and repeat buttons error.
Reupload for ckan.

# v0.9.1
Improvements :

== New UI library tools ==

a huge work have been made there !

* a new Heading selection control. It is an interactive horizontal compass.

You can use it in Attitude ans Lift pages

* New Fonts, and overall better font rendering and text alignment,
* All Editor Field : the value is sent to the mech, only if enter key is pressed or focus is changed. It avoid sending a wrong value while typing it ( It could be dangerous during flight)
  * Yellow : not validated yet
  * Red : Invalid value, it will be replaced on enter.
  * White : not focused

* Use of repeat button to fine adjust on each control that would need it

Features:

* Drone new UI and options :
  * You can choose between Altitude or V-Speed control.
  * Values are more precises using repeat buttons, text field and a slider at the same time.
  * Rool dorward direction toward current speed. It make it easier to control.
  * New UI Infos

Try the drone pilot it is more precise than ever

* Attitude Pilot :
  * like the drone pilot use of repeat buttons, text field and a slider for the elevation.
  * Added the Compass controler for the heading

# v0.9.0
Features :
* Added function to the open api : StopFlyNode and IsFlyNodeRunning. giving the ability to stop node execution
* Warp : added a little Warp to SOI Tool
* Attitude : Enhanced the Attitude controller. Is is now not only in debug mode and have a more stable way to select the direction.
  can be very useful for planes.
* Lift : direction is back in main page

Improvements :

* new UI textures for Tabs && button
* new KTools Ui made for FlightPlan
  * Added Repeat Buttons for attitude selection
  * Added new version of editor fields : the value is validated only when Enter/Return is pressed. 
  the yellow color mean than it is not yet used. No more value changes on the fly cause it could crash the vessel if wrong input were typed.

* Drone : a small Kill H-Speed tool for final precise landing.
  + Altitude Control rather than V-Speed only

Fixes :
* many little fixes when loading scene (Less NPE)

# v0.8.1
Features :
* Little open API that schlorat's [FlightPlan](https://spacedock.info/mod/3359/Flight%20Plan) will use in next version

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
* Turn phase : started the sas to maneuver mode in a proper way
* Time Warp Phase : Added Settings and a more precise safe duration before burn

# v0.2.1
* Rebuild for ksp2 v0.1.1

# v0.2.0
* added Ugly icon in the app bar and title bar
* Renamed to K2-D2

# v0.1.0

* Very first implementation.