using UnityEngine;

// tools used for the lift controller

using KTools;
using KTools.UI;
using KSP.Game;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.State;
using K2D2.KSPService;
using BepInEx.Logging;

namespace K2D2.Controller;

public class AutoLiftSettings
{
    public float heading
    {
        get => KBaseSettings.sfile.GetFloat("lift.heading", 90);
        set
        {
            value = Mathf.Clamp(value, 0 , 360);
            KBaseSettings.sfile.SetFloat("lift.heading", value);
        }
    }

    public int start_altitude_km
    {
        get => KBaseSettings.sfile.GetInt("lift.start_altitude_km", 2);
        set
        {
            // value = Mathf.Clamp(value, 0 , 1);
            KBaseSettings.sfile.SetInt("lift.start_altitude_km", value);
        }
    }

    public float mid_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.mid_rotate_ratio", 0.2f);
        set
        {
            value = Mathf.Clamp(value, 0, end_rotate_ratio);
            KBaseSettings.sfile.SetFloat("lift.mid_rotate_ratio", value);
        }
    }

    public float end_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.end_rotate_ratio", 0.5f);
        set
        {
            value = Mathf.Clamp(value, mid_rotate_ratio, 1);
            KBaseSettings.sfile.SetFloat("lift.end_rotate_ratio", value);
        }
    }

    public int destination_Ap_km
    {
        get => KBaseSettings.sfile.GetInt("lift.destination_Ap_km", 100);
        set
        {
            KBaseSettings.sfile.SetInt("lift.destination_Ap_km", value);
        }
    }
}

public class LiftAscentPath
{
    public LiftAscentPath(AutoLiftSettings lift_settings)
    {
        this.lift_settings = lift_settings;
    }

    float mid_rotate_altitude_km;
    float end_rotate_altitude_km;

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LiftAscentPath");

    public float compute_elevation(float altitude_km)
    {

        mid_rotate_altitude_km = Mathf.Lerp(lift_settings.start_altitude_km, lift_settings.destination_Ap_km, lift_settings.mid_rotate_ratio);
        end_rotate_altitude_km = Mathf.Lerp(lift_settings.start_altitude_km, lift_settings.destination_Ap_km, lift_settings.end_rotate_ratio);

        float elevation = 0;
        if (altitude_km < lift_settings.start_altitude_km)
        {
            elevation = 90;
        }
        else if (altitude_km < mid_rotate_altitude_km)
        {
            var ratio = Mathf.InverseLerp(lift_settings.start_altitude_km, mid_rotate_altitude_km, altitude_km);
            elevation = Mathf.Lerp(90, 45, ratio);
        }
        else
        {
            var ratio = Mathf.InverseLerp(mid_rotate_altitude_km, end_rotate_altitude_km, (float)altitude_km);
            elevation = Mathf.Lerp(45, 1, ratio);
        }

        return elevation;
    }


    AutoLiftSettings lift_settings;


    private Texture2D  _pathTexture = new Texture2D(1,1);
    public CelestialBodyComponent lastbody = null;
    public double last_max_alt;

    public void drawProfile()
    {
        GUILayout.Box(_pathTexture, GUILayout.Height(200));

        if (Event.current.type == EventType.Repaint) 
        {
            Rect r = GUILayoutUtility.GetLastRect();

            r.xMin += KBaseStyle.box.padding.left;
            r.yMin += KBaseStyle.box.padding.top;

            r.xMax -= KBaseStyle.box.padding.right;
            r.yMax -= KBaseStyle.box.padding.bottom;

            float scale = (float)((lift_settings.destination_Ap_km) / r.height);

            if (_pathTexture == null || _pathTexture.width !=(int) r.width || _pathTexture.height != (int)r.height)
            {
                _pathTexture = new Texture2D((int) r.width, (int)r.height);
                lastbody = null; // to rebuild atm
            }

            if (lastbody != K2D2_Plugin.Instance.current_vessel.currentBody() || last_max_alt != lift_settings.destination_Ap_km)
            {
                UpdateAtmoTexture(_pathTexture, K2D2_Plugin.Instance.current_vessel.currentBody(), lift_settings.destination_Ap_km);
            }

            DrawnPath(r, scale, scale, Color.yellow);
        }
    }



    private void DrawnPath(Rect r, float scaleX, float scaleY, Color color)
    {
        float alt = 0;
        float downrange = 0;
        var p1 = new Vector2(r.xMin, r.yMax);
        var p2 = new Vector2();

        while ((alt < lift_settings.destination_Ap_km) && (downrange < r.width * scaleX))
        {
            float desiredAngle = (float)(alt < lift_settings.start_altitude_km ? 90 : compute_elevation(alt));

            alt       += scaleY * Mathf.Sin(desiredAngle * Mathf.Deg2Rad);
            downrange += scaleX * Mathf.Cos(desiredAngle * Mathf.Deg2Rad);

            p2.x = r.xMin + downrange / scaleX;
            p2.y = r.yMax - alt / scaleY;

            if ((p1 - p2).sqrMagnitude >= 1.0)
            {
                Drawing.DrawLine(p1, p2, color, 2, true);
                p1.x = p2.x;
                p1.y = p2.y;
            }
        }
    }

    public void UpdateAtmoTexture(Texture2D texture, CelestialBodyComponent mainBody, double maxAltitude, bool realAtmo = false)
    {
        lastbody = mainBody;
        last_max_alt = maxAltitude;

        double scale = maxAltitude / texture.height; //meters per pixel

        double maxAtmosphereAltitude = mainBody.atmosphereDepth / 1000;
        double pressureSeaLevel = mainBody.atmospherePressureSeaLevel;

        for (int y = 0; y < texture.height; y++)
        {
            double alt = scale * y;
            double color_ratio;

           // logger.LogInfo($"altitude = {alt}");

            if (realAtmo)
            {
                color_ratio = mainBody.GetPressure(alt*1000) / pressureSeaLevel;
            }
            else
            {
                color_ratio = 1.0 - alt / maxAtmosphereAltitude;
            }

            float v = (float)(mainBody.hasAtmosphere ? color_ratio : 0.0F);
           // logger.LogInfo($"v value = {v}");
            var c = Color.Lerp(Color.black, Color.cyan, v);
            //var c = new Color(0.0F, 0.0F, v);

            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, c);

                if (mainBody.hasAtmosphere && (int)(maxAtmosphereAltitude / scale) == y)
                    texture.SetPixel(x, y, XKCDColors.LightGreyBlue);
            }
        }

        texture.Apply();
    }

}