using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using KTools;
// using KTools.UI;
using UnityEngine;

using K2D2.Controller;
using K2D2.Node;

namespace K2D2.Landing;

public class LandingPilot : Pilot
{
    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LandingController");

    internal LandingSettings settings;

    public static LandingPilot Instance { get; set; }

    public KSPVessel current_vessel;

    public BurndV burn_dV = new BurndV();

    public WarpTo warp_to = new WarpTo();

    public TouchDown brake = new TouchDown();

    public SingleExecuteController current_executor = new SingleExecuteController();

    public LandingPilot()
    {
        settings = new LandingSettings();
        _page = new LandingUI(this);

        Instance = this;
        debug_mode_only = false;
        name = "Land";
        K2D2PilotsMgr.Instance.RegisterPilot("Land", this);

        sub_contollers.Add(burn_dV);
        sub_contollers.Add(current_executor);

        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;
    }


    public enum Mode
    {
        Off,
        Pause,
        QuickWarp,
        RotationWarp,
        Waiting,
        Brake,
        TouchDown
    }

    public Mode mode = Mode.Off;


    double end_pause_Ut;

    public void setMode(Mode mode)
    {
        if (mode == this.mode)
            return;

        logger.LogInfo("setMode " + mode);

        this.mode = mode;

        if (mode == Mode.Off)
        {
            TimeWarpTools.SetRateIndex(0, false);
            current_executor.setController(null);
            return;
        }
        switch (mode)
        {
            case Mode.Off:
                current_executor.setController(null);
                break;
            case Mode.Pause:
                end_pause_Ut = GeneralTools.Current_UT + settings.pause_time;
                current_vessel.SetThrottle(0);
                break;
            case Mode.QuickWarp:
                current_vessel.SetThrottle(0);
                if (!settings.auto_warp.V)
                    setMode(Mode.Waiting);
                else
                {
                    current_executor.setController(warp_to);
                    warp_to.Start_Retrograde(startSafeWarp_UT);
                    warp_to.max_warp_index = 6;
                }
                break;
            case Mode.RotationWarp:
                current_vessel.SetThrottle(0);
                if (!settings.auto_warp.V)
                    setMode(Mode.Waiting);
                else
                {
                    current_executor.setController(warp_to);
                    warp_to.Start_Retrograde(startBurn_UT, true);
                    warp_to.max_warp_index = 2;
                }
                break;
            case Mode.Waiting:
                current_vessel.SetThrottle(0);
                current_executor.setController(null);
                break;
            case Mode.Brake:
            case Mode.TouchDown:
                current_executor.setController(brake);
                break;
        }

        logger.LogInfo("current_pilot " + mode);
    }

    public void nextMode()
    {
        // start
        if (mode == Mode.Off)
        {
            isRunning = true;
            return;
        }

        var next = this.mode + 1;
        setMode(next);
    }

    bool _active = false;
    public override bool isRunning
    {
        get { return _active; }
        set
        {
            if (value == _active)
                return;

            if (!value)
            {
                // stop
                if (current_vessel != null)
                    current_vessel.SetThrottle(0);

                setMode(Mode.Off);
                _active = false;
            }
            else
            {
                // Start total burn counter
                burn_dV.reset();

                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();

                _active = true;
                setMode(Mode.QuickWarp);
            }

            // send call backs
            base.isRunning = value; 
        }
    }

    public override void onReset()
    {
        isRunning = false;
    }

    internal float current_falling_speed = 0;

    internal bool collision_detected = false;

    internal double adjusted_collision_UT = 0;
    internal double startBurn_UT = 0;
    internal double startSafeWarp_UT = 0;
    internal double speed_collision;
    internal double burn_duration;

    public void computeValues()
    {
        collision_detected = false;
        var current_vessel = K2D2_Plugin.Instance.current_vessel;
        if (current_vessel == null)
        {
            // UI_Tools.Console("no vessel");
            return;
        }

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;

        collision_detected = compute_real_collision();
        speed_collision = orbit.GetOrbitalVelocityAtUTZup(adjusted_collision_UT).magnitude;
        burn_duration = (speed_collision / burn_dV.full_dv);

        compute_startBurn();
    }

    public void compute_startBurn()
    {
        startBurn_UT = adjusted_collision_UT - burn_duration - settings.burn_before.V;
        startSafeWarp_UT = startBurn_UT - settings.rotation_warp_duration.V;
    }

    public bool compute_real_collision()
    {
        // start in 2 minutes
        double start_time = GeneralTools.Game.UniverseModel.UniverseTime + 2 * 60;
        bool collide = false;

        PatchedConicsOrbit orbit = current_vessel.VesselComponent.Orbit;
        var body = orbit.referenceBody;
        double current_time_ut = GeneralTools.Game.UniverseModel.UniverseTime;
        double deltaTime = 60; // seconds in the future
        int max_occurrences = 100;
        double time = start_time;
        double terrainAltitude;

        float radius = current_vessel.VesselComponent.SimulationObject.objVesselBehavior.BoundingSphere.radius;

        for (int i = 0; i < max_occurrences; i++)
        {
            Vector3d pos;
            Vector ve;

            orbit.GetStateVectorsFromUT(time, out pos, out ve);

            Position ps = new Position(ve.coordinateSystem, pos);
            double sceneryOffset;

            body.GetAltitudeFromTerrain(ps, out terrainAltitude, out sceneryOffset);
            // terrainAltitude -= radius;

            if (terrainAltitude < 0)
            {
                collide = true;
                if (deltaTime > 0)
                {
                    // dychotomy
                    deltaTime = -deltaTime / 2;
                }
                time += deltaTime;
            }
            else
            {
                if (deltaTime < 0)
                {
                    // dychotomy
                    deltaTime = -deltaTime / 2;
                }
                time += deltaTime;
            }

            if (Math.Abs(terrainAltitude) < 1)
            {
                break;
            }
        }

        adjusted_collision_UT = time;
        return collide;
    }
    Vector SurfaceVelocity;
    public override void Update()
    {
        if (!page.isVisible && !isRunning) return;
        if (current_vessel == null || current_vessel.VesselVehicle == null)
            return;

        altitude = (float)current_vessel.GetApproxAltitude();

        SurfaceVelocity = current_vessel.VesselVehicle.SurfaceVelocity;
        SurfaceVelocity.Reframe(current_vessel.VesselVehicle.Up.coordinateSystem);
        current_falling_speed = (float)-SurfaceVelocity.vector.y;

        // detect collision and compute time to burn
        computeValues();

        if (!collision_detected)
        {
            // after patch ksp detect with altitude and remove the collision point
            if (isRunning)
            {
                setMode(Mode.TouchDown);
            }
            else
            {
                // no more collision
                isRunning = false;
            }
        }

        if (!isRunning)
            return;

        // landing detection....
        if (altitude < 5 && current_falling_speed < 1)
        {
            //current_vessel.SetThrottle(0);
            isRunning = false;
            return;
        }
        if (mode == Mode.Pause)
        {
            if (GeneralTools.Current_UT > end_pause_Ut)
            {
                setMode(Mode.QuickWarp);
            }
            return;
        }

        if (mode == Mode.QuickWarp)
        {
            warp_to.UT = startSafeWarp_UT;
        }
        else if (mode == Mode.RotationWarp)
        {
            warp_to.UT = startBurn_UT;
        }
        else if (mode == Mode.Waiting)
        {
            var dt = startBurn_UT - GeneralTools.Game.UniverseModel.UniverseTime;
            if (dt <= 0)
            {
                nextMode();
                return;
            }
        }
        else if (mode == Mode.Brake)
        {
            brake.max_speed = 0;
            brake.gravity_compensation = true;
            if (current_falling_speed < settings.brake_speed)
            {
                // we reached the speed to stop brake
                // check next phase
                if (altitude < settings.start_touchdown_altitude.V)
                {
                    setMode(Mode.TouchDown);
                }
                else
                {
                    // too high altitude retry.... very worng burn time ......
                    setMode(Mode.Pause);
                }
                return;
            }
        }
        else if (mode == Mode.TouchDown)
        {
            TimeWarpTools.SetRateIndex(0, false);
            brake.max_speed = settings.compute_limit_speed(altitude);
            brake.gravity_compensation = true;
        }

        // call the sub controllers
        base.Update();

        if (current_executor.finished)
        {
            // auto next
            nextMode();
        }
    }

    public float altitude;


}
