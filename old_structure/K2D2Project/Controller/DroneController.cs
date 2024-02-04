
using UnityEngine;

using K2D2.KSPService;
using KSP.Sim;
using KTools.UI;
using KTools;
using KSP.Tools;
using KSP.UI.Binding;
using BepInEx.Logging;

namespace K2D2.Controller;

public class DroneSettings
{
    public bool show_details
    {
        get => KBaseSettings.sfile.GetBool("drone.show_details", true);
        set
        {
            KBaseSettings.sfile.SetBool("drone.show_details", value);
        }
    }

    public float wanted_speed
    {
        get => KBaseSettings.sfile.GetFloat("drone.wanted_speed", 0);
        set
        {
            KBaseSettings.sfile.SetFloat("drone.wanted_speed", value);
        }
    }

    public float wanted_altitude
    {
        get => KBaseSettings.sfile.GetFloat("drone.wanted_altitude", 0);
        set
        {
            if (value < 0) value = 0;
            KBaseSettings.sfile.SetFloat("drone.wanted_altitude", value);
        }
    }


    public int speed_limit
    {
        get => KBaseSettings.sfile.GetInt("drone.speed_limit", 10);
        set
        {
            KBaseSettings.sfile.SetInt("drone.speed_limit", value);
        }
    }

    public float kill_h_speed_ratio
    {
        get => KBaseSettings.sfile.GetFloat("speed.kill_h_speed_ratio", 1);
        set
        {
            KBaseSettings.sfile.SetFloat("speed.kill_h_speed_ratio", value);
        }
    }

    public int altitude_ratio
    {
        get => KBaseSettings.sfile.GetInt("speed.altitude_ratio", 1);
        set
        {
            value = Mathf.Clamp(value, 1,5);
            KBaseSettings.sfile.SetInt("speed.altitude_ratio", value);
        }
    }

    public void onGUI()
    {
        UI_Tools.Title("Drone Settings");

        show_details = UI_Tools.Toggle(show_details, "Show Detailled infos");
        speed_limit = UI_Tools.IntSlider("Min-Max Speed", (int)speed_limit, 5, 100, "m/s", "just affect the Main UI");

      //  UI_Tools.Title("Altitude Settings");
        altitude_ratio = UI_Tools.IntSlider("Altitude Ratio", altitude_ratio, 1, 5, "", "adapt vSpeed on Altitude");
        UI_Tools.Right_Left_Text("Quick", "Loose");
    }
}

public class DroneController : ComplexController
{
    public static DroneController Instance { get; set; }

    DroneSettings settings = new DroneSettings();

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.DroneController");

    public SASMode sas_mode = SASMode.SurfaceUp;
    public enum SASMode { Locked, SurfaceUp, KillHSpeed }
    public static string[] sas_labels = { "Locked", "Up", "Kill Speed" };


    public VSpeedMode vspeed_mode = VSpeedMode.Direct;
    public enum VSpeedMode { Direct, Altitude }
    public static string[] vspeed_labels = { "Direct", "Altitude" };

    public ForwardDirection forward_dir = ForwardDirection.Free;
    public enum ForwardDirection { Free, Speed, Camera }
    public static string[] forward_dir_label = { "Free", "H-Speed"};

    public DroneController()
    {
        debug_mode_only = false;
        name = "Drone";
        K2D2PilotsMgr.Instance.RegisterPilot("Drone", this);

        sub_contollers.Add(burn_dV);
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
                changeSASMode( SASMode.SurfaceUp );
                changeVSpeedMode(VSpeedMode.Direct);
                changeForwardDir(ForwardDirection.Free);
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

        // convert rotation to maneuver coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, up_dir.coordinateSystem);
        Vector3d down_direction = (vessel_rotation.localRotation * Vector3.down).normalized;

        var gravity_inclination = (float)Vector3d.Angle(up_dir.vector, down_direction);
        // status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

        gravity = (float)current_vessel.VesselComponent.graviticAcceleration.magnitude;
        gravity_direction_factor = Mathf.Cos(gravity_inclination * Mathf.Deg2Rad);
    }

    float real_wanted_speed;
    float altitude;

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

        if (vspeed_mode == VSpeedMode.Direct)
            real_wanted_speed = settings.wanted_speed;
        else
        {
            altitude = (float)current_vessel.GetApproxAltitude();
            delta_altitude = settings.wanted_altitude - altitude;
            real_wanted_speed = delta_altitude / settings.altitude_ratio;
        }

        delta_speed = real_wanted_speed - current_speed;
        float remaining_full_burn_time = (float)(delta_speed / burn_dV.full_dv);

        var throttle = 0f;
        if (delta_angle_ratio <= 0)
            throttle = 1;
        else
            throttle = gravity_throttle + remaining_full_burn_time / delta_angle_ratio;

        wanted_throttle = Mathf.Clamp(throttle, 0, 1);
    }

    float delta_speed = 0;
    float delta_altitude = 0;

    float delta_angle;
    float delta_angle_ratio;

    string status_line;

    public bool checkBurnDirection()
    {
        var telemetry = SASTool.getTelemetry();

        Vector retro_dir = telemetry.HorizonUp;

        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuver coordinates
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

        gravity_compensation = true;
        current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);
        current_speed = (float)current_vessel.VesselVehicle.VerticalSpeed;

        max_DiffDirection = 85;
        status_line = "";

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
        computHSpeedHeading();
        if (sas_mode == SASMode.KillHSpeed)
        {
            // compute
            KillHSpeed();
        }

        setForwardDirection();

        base.Update();
    }


    float delta_angular_speed = 0;


    void setForwardDirection()
    {
        if (forward_dir == ForwardDirection.Free)
            return;

        if (forward_dir == ForwardDirection.Speed)
        {
            var wanted_speed = delta_forward_heading / 10;
            var angularVelocity = current_vessel.GetAngularSpeed().vector;

            delta_angular_speed = (float) (wanted_speed - angularVelocity.z);

            var roll = Mathf.Clamp(delta_angular_speed / 10, -1, 1);

            //logger.LogInfo("set roll" + roll);
            current_vessel.Roll = roll;
        }
    }

    void computHSpeedHeading()
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

        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuver coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, Upcoords);
        Vector3d forward_dir = (vessel_rotation.localRotation * Vector3.forward).normalized;

        delta_forward_heading = (float)-Vector3d.SignedAngle(forward_dir, LocalHSpeed.normalized, Up);
    }

    void KillHSpeed()
    {
        // set direction
        var autopilot = current_vessel.Autopilot;
        // force autopilot
        autopilot.Enabled = true;

        var telemetry = SASTool.getTelemetry();

        var up = telemetry.HorizonUp;

        var inc = settings.kill_h_speed_ratio * H_Speed;
        inc = Mathf.Clamp(inc, 0, 80);

        elevation = inc - 90;

        var direction = QuaternionD.Euler(elevation, retro_h_speed_heading, 0) * Vector3d.forward;

        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = false;
        autopilot.SAS.SetTargetOrientation(direction_vector, false);
    }


    float delta_forward_heading = 0;
    float h_speed_heading = 0;
    float retro_h_speed_heading = 0;

    float elevation = 0;

    void changeSASMode(SASMode new_mode)
    {
        sas_mode = new_mode;
        if (new_mode == SASMode.Locked)
        {
            SASTool.setAutoPilot(AutopilotMode.StabilityAssist);
        }
        else if (new_mode == SASMode.SurfaceUp)
        {
            SASTool.setAutoPilot(AutopilotMode.RadialOut);
        }
        else if (new_mode == SASMode.KillHSpeed)
        {
            SASTool.setAutoPilot(AutopilotMode.StabilityAssist);
            settings.kill_h_speed_ratio = 0;
        }
    }

    void changeVSpeedMode(VSpeedMode new_mode)
    {
        vspeed_mode = new_mode;
        switch(new_mode)
        {
            case VSpeedMode.Altitude:
                settings.wanted_altitude = (float)current_vessel.GetApproxAltitude();
                break;
            case VSpeedMode.Direct:
                settings.wanted_speed = 0;
                break;
        }
    }

    void changeForwardDir(ForwardDirection new_mode)
    {
        forward_dir = new_mode;
        switch(new_mode)
        {
            case ForwardDirection.Free:
                break;
            case ForwardDirection.Speed:
                break;
            case ForwardDirection.Camera:
                break;
        }
    }

    public float VSpeedControl(string ui_code, float value, float dv = 0.1f)
    {
        GUILayout.BeginHorizontal();
        UI_Tools.Label("V-Speed");
        GUILayout.FlexibleSpace();
        value = RepeatButton.OnGUI(ui_code+".minus", "--", value, -dv);
        value = UI_Fields.FloatField(ui_code+".field", value, 1);

        value = RepeatButton.OnGUI(ui_code+".plus", "++", value, dv);

        GUILayout.FlexibleSpace();
        if (UI_Tools.SmallButton(" 0 "))
            value = 0;

        GUILayout.EndHorizontal();
        return value;
    }

    public float AltitudeControl(string ui_code, float value, float dv = 0.1f)
    {
        GUILayout.BeginHorizontal();
        UI_Tools.Label("Altitude");
        GUILayout.FlexibleSpace();
        value = RepeatButton.OnGUI(ui_code+".minus", "--", value, -dv);
        value = UI_Fields.FloatField(ui_code+".field", value, 1);

        value = RepeatButton.OnGUI(ui_code+".plus", "++", value, dv);

        GUILayout.FlexibleSpace();
        if (UI_Tools.SmallButton($" {altitude:N1} "))
            value = altitude;

        GUILayout.EndHorizontal();
        return value;
    }


    public void drawDetails()
    {
         GUILayout.BeginHorizontal();

        if (V_Speed > 0)
            UI_Tools.OK($"V. Speed  : {V_Speed:n2} / {real_wanted_speed:n2} m/s");
        else
            UI_Tools.Warning($"V. Speed  : {V_Speed:n2}  / {real_wanted_speed:n2} m/s");

        GUILayout.EndHorizontal();

        if (vspeed_mode == VSpeedMode.Altitude)
        {
            UI_Tools.Console($"altitude : {altitude:n2}°");
            if (delta_altitude > 0)
                UI_Tools.OK($"d.Alt  : {delta_altitude:n2} m");
            else
                UI_Tools.Warning($"d.Alt  : {delta_altitude:n2} m");
        }

        GUILayout.BeginHorizontal();
        UI_Tools.Console($"H. Speed  : {H_Speed:n2} m/s"); 
        UI_Tools.Console($"Heading  : {h_speed_heading:n2} °");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        UI_Tools.Console($"dV consumed : {burn_dV.burned_dV:n2} m/s");
        if (UI_Tools.miniButton("Rst"))
            burn_dV.reset();
        GUILayout.EndHorizontal();
    }

    public void debugInfos()
    {
        // if (gravity_compensation)
        // {
        //     UI_Tools.Console($"gravity : {gravity:n2}");
        //     UI_Tools.Console($"gravity inclination : {gravity_inclination:n2}°");
        //     //GUILayout.Label($"gravity_direction_factor : {gravity_direction_factor:n2}");
        // }

        UI_Tools.Console($"wanted V speed  : {real_wanted_speed:n2}  m/s");

        UI_Tools.Console($"delta speed  : {delta_speed:n2}  m/s");
        UI_Tools.Console($"delta_angle_ratio : {delta_angle_ratio:n2}");
        UI_Tools.Console($"wanted_throttle : {wanted_throttle:n2}");

        if (sas_mode == SASMode.KillHSpeed)
        {
            UI_Tools.Console($"retro_h_speed_heading  : {retro_h_speed_heading:n2} °");
            UI_Tools.Console($"elevation : {elevation:n2}°");
        }

        if (forward_dir ==  ForwardDirection.Speed)
        {
            UI_Tools.Console($"h_speed_heading  : {h_speed_heading:n2} °");
            UI_Tools.Console($"forward_heading  : {delta_forward_heading:n2} °");
            var angularVelocity = current_vessel.GetAngularSpeed().vector;

            UI_Tools.Console($"angularVelocity  : {  StrTool.Vector3ToString(angularVelocity)  } °");
            UI_Tools.Console($"delta_angular_speed  : {delta_angular_speed:n2} °/s");
        }
    }

    public override void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            settings.onGUI();
            return;
        }

        UI_Tools.Title("Drone Pilot");

        isRunning = UI_Tools.BigToggleButton(isRunning, "Run", "Stop");
        if (!isRunning)
            return;

                var new_sas_mode = UI_Tools.EnumGrid<SASMode>("SAS Mode", sas_mode, sas_labels);
        if (new_sas_mode != sas_mode)
        {
           changeSASMode(new_sas_mode);
        }

        if (sas_mode == SASMode.KillHSpeed)
        {
            UI_Tools.Label($"Kill H_speed");
            settings.kill_h_speed_ratio = UI_Tools.FloatSliderTxt("Kill Speed Ratio", settings.kill_h_speed_ratio, 0, 10, "", "", 0);
        }

        var new_vspeed_mode = UI_Tools.EnumGrid<VSpeedMode>("V Speed", vspeed_mode, vspeed_labels);
        if (new_vspeed_mode != vspeed_mode)
        {
            changeVSpeedMode(new_vspeed_mode);
        }

        if (vspeed_mode == VSpeedMode.Direct)
        {
            settings.wanted_speed = VSpeedControl("drone.wanted_speed", settings.wanted_speed, 0.1f);
            settings.wanted_speed = UI_Tools.FloatSlider(settings.wanted_speed, -settings.speed_limit, settings.speed_limit);
        }
        else if (vspeed_mode == VSpeedMode.Altitude)
        {
            settings.wanted_altitude = AltitudeControl("drone.wanted_altitude", settings.wanted_altitude, 0.5f);
            settings.wanted_altitude = UI_Tools.FloatSlider(settings.wanted_altitude, 0, 500);
        }

        var new_forward_dir = UI_Tools.EnumGrid<ForwardDirection>("Forward Direction", forward_dir, forward_dir_label);
        if (new_forward_dir != forward_dir)
        {
            changeForwardDir(new_forward_dir);
        }

        if (settings.show_details)
            drawDetails();

        if (K2D2Settings.debug_mode)
        {
            debugInfos();
        }

        if (!string.IsNullOrEmpty(status_line))
        {
            GUILayout.Label(status_line);
        }
    }
}
