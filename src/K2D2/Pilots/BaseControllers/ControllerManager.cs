using System.Collections.Generic;
using System.Linq;
using K2D2.Controller;

namespace K2D2.Controller
{
    public class PilotsManager
    {
        public List<Pilot> pilots = new List<Pilot>();

        public void AddPilot(Pilot controller)
        {
            pilots.Add(controller);
        }

        public void onReset()
        {
            foreach (var controller in pilots)
            {
                controller.onReset();
            }
        }


        /// <summary>
        /// Calls the Update() method of all controllers
        /// </summary>
        public void UpdateControllers()
        {
            foreach (var controller in pilots)
            {
                controller.Update();
            }
        }

        /// <summary>
        /// Calls the LateUpdate() method of all controllers
        /// </summary>
        public void LateUpdateControllers()
        {
            foreach (var controller in pilots)
            {
                controller.LateUpdate();
            }
        }

          /// <summary>
        /// Calls the Update() method of all controllers
        /// </summary>
        public void FixedUpdateControllers()
        {
            foreach (var controller in pilots)
            {
                controller.FixedUpdate();
            }
        }

    }
}