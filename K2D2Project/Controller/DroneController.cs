
using UnityEngine;

using K2D2.KSPService;
using KSP.Sim;
using KTools.UI;
using KTools;

namespace K2D2.Controller;

public class DroneSettings
{
    public DroneController.SpeedMode speed_mode
    {
        get => KBaseSettings.sfile.GetEnum<DroneController.SpeedMode>("speed.direction", 0);
        set
        {
            KBaseSettings.sfile.SetEnum<DroneController.SpeedMode>("speed.direction", value);
        }
    }

    public float wanted_speed
    {
        get => KBaseSettings.sfile.GetFloat("speed.wanted_speed", 0);
        set
        {
            KBaseSettings.sfile.SetFloat("speed.wanted_speed", value);
        }
    }

    public int speed_limit
    {
        get => KBaseSettings.sfile.GetInt("speed.speed_limit", 10);
        set
        {
            KBaseSettings.sfile.SetInt("speed.speed_limit", value);
        }
    }

    public bool sas_up
    {
        get => KBaseSettings.sfile.GetBool("speed.lock_sas", true);
        set
        {
            KBaseSettings.sfile.SetBool("speed.lock_sas", value);
        }
    }

    public bool kill_h_speed
    {
        get => KBaseSettings.sfile.GetBool("speed.kill_h_speed", true);
        set
        {
            KBaseSettings.sfile.SetBool("speed.kill_h_speed", value);
        }
    }

    public float inclinaison_ratio
    {
        get => KBaseSettings.sfile.GetFloat("speed.inclinaison_ratio", 1);
        set
        {
            KBaseSettings.sfile.SetFloat("speed.inclinaison_ratio", value);
        }
    }

    public void onGUI()
    {
        UI_Tools.Title("Speed Controller Settings");
        speed_limit = UI_Tools.IntSlider("Min-Max Speed", (int)speed_limit, 5, 100, "m/s", "just affect the Main UI");

        UI_Tools.Title($"Kill H_speed");
        inclinaison_ratio = UI_Tools.FloatSliderTxt("Kill Speed Ratio", inclinaison_ratio, 0, 10, "", "", 0);
    }
}

public class DroneController : ComplexControler
{
    public static DroneController Instance { get; set; }

    public enum SpeedMode { SurfaceUp, SurfaceSpeed }
    public static string[] mode_labels = { "V-Speed", "Brake" };

    DroneSettings settings = new DroneSettings();


    public DroneController()
    {
        debug_mode_only = false;
        name = "Drone";

        sub_contollers.Add(burn_dV);
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
    }

    public override void onReset()
    {
        isRunning = false;
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
                var current_vessel = K2D2_Plugin.Instance.current_vessel;
                if (current_vessel != null)
                    current_vessel.SetThrottle(0);

                _active = false;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();
                _active = true;
            }
        }
    }

    float max_DiffDirection = 90;

    public bool gravity_compensation;
    public float V_Speed = 0;
    public float H_Speed = 0;

    KSPVessel current_vessel;
    BurndV burn_dV = new BurndV();

    float gravity_direction_factor = 0;
    float gravity;

    float wanted_throttle = 0;

    public void computeGravityRatio()
    {
        // current_vessel.getInclination();
        Vector up_dir = current_vessel.VesselComponent.gravityForPos;
        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuvre coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, up_dir.coordinateSystem);
        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.down).normalized;

        var gravity_inclination = (float)Vector3d.Angle(up_dir.vector, forward_direction);
        // status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

        gravity = (float)current_vessel.VesselComponent.graviticAcceleration.magnitude;
        gravity_direction_factor = Mathf.Cos(gravity_inclination * Mathf.Deg2Rad);
    }

    void compute_Throttle()
    {
        float gravity_throttle = 0;

        if (gravity_compensation)
        {
            float minimum_dv = 0;
            if (gravity_direction_factor > 0)
                minimum_dv = gravity / gravity_direction_factor;
            else
                minimum_dv = 0;

            gravity_throttle = minimum_dv / burn_dV.full_dv;
        }

        delta_speed = settings.wanted_speed - current_speed;
        float remaining_full_burn_time = (float)(delta_speed / burn_dV.full_dv);

        var throttle = 0f;
        if (delta_angle_ratio <= 0)
            throttle = 1;
        else
            throttle = gravity_throttle + remaining_full_burn_time / delta_angle_ratio;

        wanted_throttle = Mathf.Clamp(throttle, 0, 1);
    }

    float delta_speed = 0;

    float delta_angle;
    float delta_angle_ratio;

    string status_line;

    public bool checkBurnDirection()
    {
        var telemetry = SASTool.getTelemetry();
        Vector retro_dir = default(Vector);

        switch (settings.speed_mode)
        {
            case SpeedMode.SurfaceSpeed:
                retro_dir = telemetry.SurfaceMovementRetrograde;
                break;
            case SpeedMode.SurfaceUp:
                retro_dir = telemetry.HorizonUp;
                break;
        }

        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuvre coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, retro_dir.coordinateSystem);
        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

        delta_angle = (float)Vector3d.Angle(retro_dir.vector, forward_direction);
        delta_angle_ratio = Mathf.Cos(delta_angle * Mathf.Deg2Rad);
        if (delta_angle < max_DiffDirection)
            return true;
        else
        {
            status_line = $"Waiting for good sas direction\nAngle = {delta_angle:n2}° > {max_DiffDirection:n2}°";
            return false;
        }
    }

    float current_speed;


    public override void Update()
    {
        if (!ui_visible && !isRunning) return;
        if (current_vessel == null || current_vessel.VesselVehicle == null)
            return;

        V_Speed = (float)current_vessel.VesselVehicle.VerticalSpeed;
        H_Speed = (float)current_vessel.VesselVehicle.HorizontalSpeed;
        if (!isRunning) return;

        switch (settings.speed_mode)
        {
            case SpeedMode.SurfaceSpeed:
                gravity_compensation = true;
                current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
                current_speed = -(float)current_vessel.VesselVehicle.SurfaceSpeed;

                if (settings.sas_up)
                    SASTool.setAutoPilot(AutopilotMode.Retrograde);

                max_DiffDirection = 85;
                status_line = $"Max Speed : {settings.wanted_speed:n2} m/s";

                break;
            case SpeedMode.SurfaceUp:
                gravity_compensation = true;
                current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
                current_speed = (float)current_vessel.VesselVehicle.VerticalSpeed;

                if (settings.sas_up)
                    SASTool.setAutoPilot(AutopilotMode.RadialOut);

                max_DiffDirection = 85;
                status_line = "";
                break;
        }

        if (gravity_compensation)
            computeGravityRatio();

        if (!checkBurnDirection())
        {
            current_vessel.SetThrottle(0);
            status_line = $"Turning : {delta_angle:n2} °";
            return;
        }

        compute_Throttle();

        // no stop for gravity compensation
        current_vessel.SetThrottle(wanted_throttle);
        computHeading();
        if (settings.kill_h_speed)
        {
            // compute
            KillHSpeed();
        }

        base.Update();
    }

    void computHeading()
    {
        // use Up local coordinate as reference frame
        var Upcoords = current_vessel.VesselVehicle.Up.coordinateSystem;
        var SurfaceVelocity = Vector.Reframed(current_vessel.VesselVehicle.SurfaceVelocity, Upcoords).vector;
        var North = Vector.Reframed(current_vessel.VesselVehicle.North, Upcoords).vector;
        var Up = current_vessel.VesselVehicle.Up.vector;

        var UpSpeed = Up.normalized * Vector3d.Dot(SurfaceVelocity, Up);
        var LocalHSpeed = SurfaceVelocity - UpSpeed;

        // UI_Tools.Console($"UpSpeed : {StrTool.VectorToString(UpSpeed)}");
        // UI_Tools.Console($"LocalHSpeed : {StrTool.VectorToString(LocalHSpeed)}");
        h_speed_heading = (float)-Vector3d.SignedAngle(LocalHSpeed.normalized, North, Up);
        retro_h_speed_heading = (float)-Vector3d.SignedAngle(-LocalHSpeed.normalized, North, Up);
    }

    void KillHSpeed()
    {
        // set direction
        var autopilot = current_vessel.Autopilot;
        // force autopilot
        autopilot.Enabled = true;

        var telemetry = SASTool.getTelemetry();

        var up = telemetry.HorizonUp;

        var inc = settings.inclinaison_ratio * H_Speed;
        inc = Mathf.Clamp(inc, 0, 80);

        elevation = inc - 90;

        var direction = QuaternionD.Euler(elevation, retro_h_speed_heading, 0) * Vector3d.forward;

        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = false;
        autopilot.SAS.SetTargetOrientation(direction_vector, false);
    }

    float h_speed_heading = 0;
    float retro_h_speed_heading = 0;

    float elevation = 0;

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            settings.onGUI();
            return;
        }

        // UI_Tools.Title("Vertical Speed controller");
        // settings.speed_mode = UI_Tools.EnumGrid<SpeedMode>("Mode",
        //                                         settings.speed_mode, mode_labels);


        GUILayout.BeginHorizontal();
        settings.speed_mode = SpeedMode.SurfaceUp;
        var lock_sas = UI_Tools.Toggle(settings.sas_up, "SAS UP");
        if (lock_sas != settings.sas_up)
        {
            if (lock_sas)
                settings.kill_h_speed = false;
            else
                // unlock the SAS goes to stability assist
                SASTool.setAutoPilot(AutopilotMode.StabilityAssist);
            settings.sas_up = lock_sas;
        }

        var kill_h_speed = UI_Tools.Toggle(settings.kill_h_speed, "Kill H Speed");
        if (kill_h_speed != settings.kill_h_speed)
        {
            if (kill_h_speed)
            {
                settings.sas_up = false;
            }
            else
                SASTool.setAutoPilot(AutopilotMode.RadialOut);

            SASTool.setAutoPilot(AutopilotMode.StabilityAssist);
            settings.kill_h_speed = kill_h_speed;
        }
    
        GUILayout.EndHorizontal();

        if (settings.speed_mode == SpeedMode.SurfaceSpeed)
        {
            settings.wanted_speed = Mathf.Min(settings.wanted_speed, -0.5f);
            settings.wanted_speed = UI_Tools.FloatSliderTxt("Limit Speed ", settings.wanted_speed, -settings.speed_limit, -0.5f, "m/s");
            UI_Tools.Right_Left_Text($"{-settings.speed_limit:n2}", $"{-0.5f:n2}");
        }
        else
        {
            settings.wanted_speed = UI_Tools.FloatSliderTxt("V-Speed ", settings.wanted_speed, -settings.speed_limit, settings.speed_limit, "m/s");
            UI_Tools.Right_Left_Text($"{-settings.speed_limit:n2}", $"{settings.speed_limit:n2}");

            if (Mathf.Abs(settings.wanted_speed) < settings.speed_limit / 10)
                settings.wanted_speed = 0;
        }

        if (Mathf.Abs(settings.wanted_speed) < 0.3f)
            settings.wanted_speed = 0;

        isRunning = UI_Tools.BigToggleButton(isRunning, "Run", "Stop");
        if (!isRunning)
            return;

        UI_Tools.Console($"V. Speed  : {V_Speed:n2} m/s");
        GUILayout.BeginHorizontal();
        UI_Tools.Console($"H. Speed  : {H_Speed:n2} m/s"); 
        UI_Tools.Console($"Heading  : {h_speed_heading:n2} °");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        UI_Tools.Console($"dV consumed : {burn_dV.burned_dV:n2} m/s");
        if (UI_Tools.miniButton("Rst"))
            burn_dV.reset();
        GUILayout.EndHorizontal();

        if (K2D2Settings.debug_mode)
        {
            // if (gravity_compensation)
            // {
            //     UI_Tools.Console($"gravity : {gravity:n2}");
            //     UI_Tools.Console($"gravity inclination : {gravity_inclination:n2}°");
            //     //GUILayout.Label($"gravity_direction_factor : {gravity_direction_factor:n2}");
            // }

            UI_Tools.Console($"delta speed  : {delta_speed:n2}  m/s");
            UI_Tools.Console($"delta_angle_ratio : {delta_angle_ratio:n2}");
            UI_Tools.Console($"wanted_throttle : {wanted_throttle:n2}");

            if (settings.kill_h_speed)
            {
                UI_Tools.Console($"retro_h_speed_heading  : {retro_h_speed_heading:n2} °");
                UI_Tools.Console($"elevation : {elevation:n2}°");
            }
        }

        if (!string.IsNullOrEmpty(status_line))
        {
            GUILayout.Label(status_line);
        }
    }
}
