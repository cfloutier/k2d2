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
            _mid_rotate_altitude_km = -1;
            _end_rotate_altitude_km = -1;
        }
    }

    public float mid_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.mid_rotate_ratio", 0.2f);
        set
        {
            value = Mathf.Clamp(value, 0, end_rotate_ratio);
            KBaseSettings.sfile.SetFloat("lift.mid_rotate_ratio", value);
            _mid_rotate_altitude_km = -1;
        }
    }


    float _mid_rotate_altitude_km = -1;
    public float mid_rotate_altitude_km
    {
        get {
            if (_mid_rotate_altitude_km == -1)
                _mid_rotate_altitude_km = Mathf.Lerp(start_altitude_km, destination_Ap_km, mid_rotate_ratio);

            return _mid_rotate_altitude_km;
        }
    }

    public float end_rotate_ratio
    {
        get => KBaseSettings.sfile.GetFloat("lift.end_rotate_ratio", 0.5f);
        set
        {
            value = Mathf.Clamp(value, mid_rotate_ratio, 1);
            KBaseSettings.sfile.SetFloat("lift.end_rotate_ratio", value);
            _mid_rotate_altitude_km = -1;
        }
    }

    float _end_rotate_altitude_km = -1;
    public float end_rotate_altitude_km
    {
         get {
            if (_end_rotate_altitude_km == -1)
                _end_rotate_altitude_km = Mathf.Lerp(start_altitude_km, destination_Ap_km, end_rotate_ratio);

            return _end_rotate_altitude_km;
        }
    }

    public int destination_Ap_km
    {
        get => KBaseSettings.sfile.GetInt("lift.destination_Ap_km", 100);
        set
        {
            KBaseSettings.sfile.SetInt("lift.destination_Ap_km", value);
            _end_rotate_altitude_km = -1;
            _mid_rotate_altitude_km = -1;

        }
    }
}

public class LiftAscentPath
{
    public LiftAscentPath(AutoLiftSettings lift_settings)
    {
        this.lift_settings = lift_settings;
    }

    public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.LiftAscentPath");

    public float compute_elevation(float altitude_km)
    {
        float elevation = 0;
        if (altitude_km < lift_settings.start_altitude_km)
        {
            elevation = 90;
        }
        else if (altitude_km < lift_settings.mid_rotate_altitude_km)
        {
            var ratio = Mathf.InverseLerp(lift_settings.start_altitude_km, lift_settings.mid_rotate_altitude_km, altitude_km);
            elevation = Mathf.Lerp(90, 45, ratio);
        }
        else
        {
            var ratio = Mathf.InverseLerp(lift_settings.mid_rotate_altitude_km, lift_settings.end_rotate_altitude_km, (float)altitude_km);
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

            float scale = (float)(lift_settings.destination_Ap_km / r.height);

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

    private void DrawLines(Rect r, float scaleY)
    {
        var p1 = new Vector2(r.xMin, 0);
        var p2 = new Vector2(r.xMax, 0);

        p1.y = p2.y = lift_settings.start_altitude_km / scaleY;
        Drawing.DrawLine(p1, p2, Color.blue, 2, true);

        p1.y = p2.y = lift_settings.mid_rotate_altitude_km / scaleY;
        Drawing.DrawLine(p1, p2, Color.blue, 2, true);

        if (maxAtmosphereAltitude_km > 0)
        {
            p1.y = p2.y = maxAtmosphereAltitude_km / scaleY;
            Drawing.DrawLine(p1, p2, Color.red, 2, true);
        }
    }

    float maxAtmosphereAltitude_km = -1;

    public void UpdateAtmoTexture(Texture2D texture, CelestialBodyComponent mainBody, double maxAltitude)
    {
        lastbody = mainBody;
        last_max_alt = maxAltitude;

        double scale = maxAltitude / texture.height; //meters per pixel

        if (mainBody.hasAtmosphere)
            maxAtmosphereAltitude_km = (float) (mainBody.atmosphereDepth / 1000);
        else
            maxAtmosphereAltitude_km = -1;

        double pressureSeaLevel = mainBody.atmospherePressureSeaLevel;

        for (int y = 0; y < texture.height; y++)
        {
            double alt = scale * y;

            float atmo_ratio = (float)(mainBody.GetPressure(alt*1000) / pressureSeaLevel);
            float altitude_atm_ratio = mainBody.hasAtmosphere ?  (float)(1.0 - alt / maxAtmosphereAltitude_km) : 0.0f;

            var c = Color.Lerp(Color.black, Color.cyan, altitude_atm_ratio) + new Color(atmo_ratio, atmo_ratio, atmo_ratio, 1);

            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, c);

                // if (mainBody.hasAtmosphere && (int)(maxAtmosphereAltitude_km / scale) == y)
                //     texture.SetPixel(x, y, XKCDColors.LightGreyBlue);
            }
        }

        texture.Apply();
    }

}