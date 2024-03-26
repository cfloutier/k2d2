using K2D2.KSPService;
using K2UI;
using KSP.Sim;
// using KTools.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace K2D2.Controller;

/// apply the wanted speed in the good direction
public class TouchDown : ExecuteController
{
    public bool gravity_compensation;
    public float max_speed = 0;

    KSPVessel current_vessel;
    BurndV burn_dV = new BurndV();

    // float gravity_inclination = 0;
    float gravity_direction_factor = 0;
    float gravity;

    float wanted_throttle = 0;

    public TouchDown()
    {
        sub_contollers.Add(burn_dV);
        // logger.LogMessage("LandingController !");
        current_vessel = K2D2Plugin.Instance.current_vessel;
    }

    public void computeGravityRatio()
    {
        // current_vessel.getInclination();
        Vector up_dir = current_vessel.VesselComponent.gravityForPos;
        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuver coordinates

        vessel_rotation = Rotation.Reframed(vessel_rotation, up_dir.coordinateSystem);
        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.down).normalized;

        var gravity_inclination = (float)Vector3d.Angle(up_dir.vector, forward_direction);
        // status_line = $"Waiting for good sas direction\nAngle = {angle:n2}°";

        gravity = (float)current_vessel.VesselComponent.graviticAcceleration.magnitude;
        gravity_direction_factor = Mathf.Cos(gravity_inclination * Mathf.Deg2Rad);
    }

    void compute_Throttle()
    {
        float min_throttle = 0;

        if (gravity_compensation)
        {
            if (gravity_direction_factor == 0)
                min_throttle = 0;
            else
            {
                float minimum_dv = gravity_direction_factor * gravity;
                min_throttle = minimum_dv / burn_dV.full_dv;
            }
        }


        delta_speed = current_speed - max_speed;

        float remaining_full_burn_time = (float)(delta_speed / burn_dV.full_dv);
        wanted_throttle = Mathf.Clamp(remaining_full_burn_time + min_throttle, 0, 1);
    }

    float delta_speed = 0;

    public bool NeedBurn => delta_speed > 0;

    public bool checkSpeed()
    {
        if (delta_speed < 0)
        {
            return true;
        }

        return false;
    }

    float retrograde_angle;

    public bool checkDirection()
    {
        double max_angle = 5;

        var telemetry = SASTool.getTelemetry();

        // check that the speed
        Vector HorizonUp = telemetry.HorizonUp;
        Vector retro_dir = telemetry.SurfaceMovementRetrograde;

        retro_dir.Reframe(HorizonUp.coordinateSystem);

        var speed_vertical_angle = (float)Vector3d.Angle(retro_dir.vector, HorizonUp.vector);

        if (speed_vertical_angle > 90)
        {
            status_line = $"Waiting for speed Down\nAngle = {speed_vertical_angle:n2}°\nFree Time Warp";
            return false;
        }

        TimeWarpTools.SetRateIndex(0, false);
        SASTool.setAutoPilot(AutopilotMode.Retrograde);

        Rotation vessel_rotation = current_vessel.GetRotation();

        // convert rotation to maneuver coordinates
        vessel_rotation = Rotation.Reframed(vessel_rotation, retro_dir.coordinateSystem);
        Vector3d forward_direction = (vessel_rotation.localRotation * Vector3.up).normalized;

        retrograde_angle = (float)Vector3d.Angle(retro_dir.vector, forward_direction);
        status_line = $"Waiting for Vessel rotation\nAngle = {retrograde_angle:n2}°";

        return retrograde_angle < max_angle;
    }

    float current_speed;

    public override void Update()
    {
        if (current_vessel == null || current_vessel.VesselVehicle == null)
            return;

        current_speed = (float)current_vessel.VesselVehicle.SurfaceSpeed;

        delta_speed = current_speed - max_speed;

        if (delta_speed > 0) // reset timewarp if it is time to burn
            TimeWarpTools.SetRateIndex(0, false);

        if (gravity_compensation)
            computeGravityRatio();

        current_vessel.SetSpeedMode(KSP.Sim.SpeedDisplayMode.Surface);

        // if (autopilot.AutopilotMode != AutopilotMode.Retrograde)
        //         autopilot.SetMode(AutopilotMode.Retrograde);
        // else


        // if (autopilot.AutopilotMode != AutopilotMode.Retrograde)
        //     autopilot.SetMode(AutopilotMode.Retrograde);
        // else
        //     if (autopilot.AutopilotMode != AutopilotMode.StabilityAssist)
        //         autopilot.SetMode(AutopilotMode.StabilityAssist);

        if (!checkDirection())
        {
            current_vessel.SetThrottle(0);
            // status_line = $"Turning : {retrograde_angle:n2} °";
            return;
        }

        compute_Throttle();
        status_line = $"Max Speed : {max_speed:n2} m/s";

        // no stop for gravity compensation
        current_vessel.SetThrottle(wanted_throttle);
    }

    public override void updateUI(VisualElement el, FullStatus st)
    {
        // need to burn ?

        string txt = $" Max speed : {max_speed:n2} !!";
        txt += $"\n delta speed  : {delta_speed:n2}  m/s";    

        var level = delta_speed > 0 ? StatusLine.Level.Warning : StatusLine.Level.Normal;
        st.Status(txt, level );
       
        if (burn_dV.burned_dV > 0)
            st.Console($"dV consumed : {burn_dV.burned_dV:n2} m/s");

        if (K2D2Settings.debug_mode.V)
        {
            if (gravity_compensation)
            {
                st.Console($"gravity : {gravity:n2}");
                st.Console($"gravity_direction_factor : {gravity_direction_factor:n2}");   
            }

            st.Console($"wanted_throttle : {wanted_throttle:n2}");
        }
    }


}

