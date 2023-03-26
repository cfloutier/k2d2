using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.Messages;

namespace K2D2.Controller
{
    // a controller that have some sub controler
    // contains also an active flag,
    public class ComplexControler : BaseController
    {
        public bool is_active = true;

        public List<ComplexControler> sub_contollers = new List<ComplexControler>();

        public void setSingleSubController(ComplexControler single_sub)
        {
            sub_contollers.Clear();
            if (single_sub != null)
                sub_contollers.Add(single_sub);
        }

        public override void onGUI()
        {
            // On GUI is used to draw UI in needed, using GUILayout
            if (!is_active) return;
            foreach (ComplexControler contoller in sub_contollers)
            {
                contoller.onGUI();
            }
        }

        public override void Update()
        {
            // Update is called each frame
            if (!is_active) return;
            foreach (ComplexControler contoller in sub_contollers)
            {
                contoller.Update();
            }
        }

        public override void LateUpdate()
        {
            // Late Update is called just before rendering
            if (!is_active) return;
            foreach (ComplexControler contoller in sub_contollers)
            {
                contoller.LateUpdate();
            }
        }

        public override void FixedUpdate()
        {
            // Fixed Update is called on physic update
            if (!is_active) return;
            foreach (ComplexControler contoller in sub_contollers)
            {
                contoller.FixedUpdate();
            }
        }
    }

}


