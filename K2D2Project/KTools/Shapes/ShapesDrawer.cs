
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using UnityEngine.Rendering;

using KSP.Game;
using KSP.Messages;
using K2D2;

namespace KTools.Shapes
{

    public class ShapeDrawer
    {
        public delegate void onDrawShape();

        public static ShapeDrawer Instance { get; set; }

        public ShapeDrawer()
        {
            Instance = this;
        }
        public List<onDrawShape> shapes = new List<onDrawShape>();

        public void DrawShapes(Camera cam)
        {
            if (cam == null)
            {
                return;
            }

            //K2D2_Plugin.logger.LogInfo("DrawShapes " + cam.name);

            if (cam.name != "FlightCameraPhysics_Main")
            {
                return;
            }

            //K2D2_Plugin.logger.LogInfo("can_draw " + can_draw);

            if (!can_draw)
            {
                return;
            }

            using (Draw.Command(cam, CameraEvent.AfterForwardAlpha))
            {
                Draw.ResetMatrix();
                foreach (onDrawShape fct in shapes)
                {
                    fct();
                }
            }
        }

        public void OnPostRender(Camera cam)
        {
            if (!can_draw)
            {
                return;
            }

            DrawCommand.OnPostRenderBuiltInRP(cam);
        }

        public bool can_draw = false;

    }
}