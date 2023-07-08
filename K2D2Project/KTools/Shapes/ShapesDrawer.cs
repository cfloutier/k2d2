
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using UnityEngine.Rendering;

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
            using (Draw.Command(cam, CameraEvent.AfterForwardAlpha))
            {
                Draw.ResetMatrix();
                foreach (onDrawShape fct in shapes)
                {
                    fct();
                }
            }
        }

        public static void OnPostRender(Camera cam)
        {
            DrawCommand.OnPostRenderBuiltInRP(cam);
        }
    }
}