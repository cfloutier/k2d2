using BepInEx.Logging;
using K2D2.KSPService;
using KSP.Sim;
using KSP.UI.Binding;
using KTools;
using KTools.UI;
using UnityEngine;
using static RTG.CameraFocus;
using static System.Net.Mime.MediaTypeNames;

namespace K2D2.Controller;

public class RepeatButton
{
    class ButtonInstance
    {
        public ButtonInstance(string name)
        {
            this.name = name;
        }

        public string name;
        public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("RepeatButton");
        public bool is_active = false;
        public float next_time;
        public float delta_time = 0;


        void log(string txt)
        {
            if (name == "elevation_minus")
            {
                logger.LogInfo(txt);
            }
        }

        public float run(string txt, float value, float delta)
        {

            bool is_On = GUILayout.RepeatButton(txt, KBaseStyle.small_button);

            if (Event.current.type == EventType.Repaint)
            {
                if (is_On)
                {
                 
                    if (!is_active)
                    {
                        is_active = true;
                        delta_time = start_delta_time;
                        next_time = Time.time + delta_time;

                        value += delta;
                    }
                    else if (Time.time > next_time)
                    {
                        delta_time = delta_time * 0.9f;
                        if (delta_time < 0.1f)
                            delta_time = 0.1f;
                        next_time = Time.time + delta_time;

                        value += delta;
                    }
                }
                else
                {
                    is_active = false;
              
                }
            }
           

            return value;

        }
    }

    static Dictionary<string, ButtonInstance> instances = new Dictionary<string, ButtonInstance>();

    static float start_delta_time = 0.5f;
    

   
    

    public static float Draw(string instance_name, string txt, float value, float delta)
    {
        ButtonInstance instance = null;
        if (!instances.ContainsKey(instance_name))
        {
            instance = new ButtonInstance(instance_name);
            instances[instance_name] = instance;
        }
        else
            instance = instances[instance_name];

       
        return instance.run(txt, value, delta);

    }


}


class AttitudeSettings
{

    public static float elevation
    {
        get => KBaseSettings.sfile.GetFloat("warp.elevation", 0);
        set
        {
            value = Mathf.Clamp(value, -90, 90);
            KBaseSettings.sfile.SetFloat("warp.elevation", value);
        }
    }

    public static float heading
    {
        get => KBaseSettings.sfile.GetFloat("warp.heading", 0);
        set
        {
            value = Mathf.Clamp(value, -180, 180);
            KBaseSettings.sfile.SetFloat("warp.heading", value);
        }
    }

}

public class AttitudeController : ComplexControler
{
    public static AttitudeController Instance { get; set; }

    KSPVessel current_vessel;

    public AttitudeController()
    {
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2_Plugin.Instance.current_vessel;

        Instance = this;
        debug_mode_only = false;
        name = "Attitude";
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
                // var current_vessel = K2D2_Plugin.Instance.current_vessel;
                // if (current_vessel != null)
                //     current_vessel.SetThrottle(0);

                _active = false;
            }
            else
            {
                // reset controller to desactivate other controllers.
                K2D2_Plugin.ResetControllers();
                _active = true;

                var autopilot = current_vessel.Autopilot;

                // force autopilot
                autopilot.Enabled = true;
                autopilot.SetMode(AutopilotMode.StabilityAssist);
            }
        }
    }



    Vector3d direction = Vector3d.zero;

    public override void Update()
    {
        if (!isRunning) return;

        if (current_vessel == null) return;
        var autopilot = current_vessel.Autopilot;

        // force autopilot
        autopilot.Enabled = true;

        var telemetry = SASTool.getTelemetry();

        var up = telemetry.HorizonUp;

        direction = QuaternionD.Euler(AttitudeSettings.elevation, AttitudeSettings.heading, 0) * Vector3d.forward;

        Vector direction_vector = new Vector(up.coordinateSystem, direction);

        autopilot.SAS.lockedMode = false;
        autopilot.SAS.SetTargetOrientation(direction_vector, false);
    }





    public override async void onGUI()
    {
        if (K2D2_Plugin.Instance.settings_visible)
        {
            K2D2Settings.onGUI();
            return;
        }


        GUILayout.BeginHorizontal();
        UI_Tools.Label("Elevation");
        GUILayout.FlexibleSpace();

        AttitudeSettings.elevation = RepeatButton.Draw("elevation_minus", " -- ", AttitudeSettings.elevation, -0.2f);

        GUILayout.Label($"{AttitudeSettings.elevation:N1} °", KBaseStyle.value_field,  GUILayout.Width(100));

        AttitudeSettings.elevation = RepeatButton.Draw("elevation_plus", " ++ ", AttitudeSettings.elevation, 0.2f);

        //AttitudeSettings.elevation = UI_Fields.FloatMinMaxField("attitude.elevationField", AttitudeSettings.elevation, -90, 90);
        //UI_Tools.Label("°");
        GUILayout.EndHorizontal();
        GUI.SetNextControlName("attitude.elevationSlider");
        AttitudeSettings.elevation = UI_Tools.FloatSlider( AttitudeSettings.elevation, -90, 90);
        AttitudeSettings.heading = UI_Tools.HeadingSlider("heading", AttitudeSettings.heading);

        // UI_Tools.ProgressBar(heading, -180, 180);

        // z_direction = UI_Tools.FloatSlider("Z", z_direction, -180, 180, "°");
    //    if (Mathf.Abs(AttitudeSettings.elevation) < 2) AttitudeSettings.elevation = 0;
        // if (Mathf.Abs(z_direction) < 2) z_direction = 0;

        isRunning = UI_Tools.BigToggleButton(isRunning, "Start", "Stop");

     

    }


}
