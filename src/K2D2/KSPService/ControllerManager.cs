using System.Collections.Generic;
using System.Linq;
using K2D2.Controller;

namespace K2D2.Models
{
    public class ControllerManager
    {
        public List<BaseController> controllers = new List<BaseController>();

        public void AddController(BaseController controller)
        {
            controllers.Add(controller);
        }

        public void onReset()
        {
            foreach (var controller in controllers)
            {
                controller.onReset();
            }
        }


        /// <summary>
        /// Calls the Update() method of all controllers
        /// </summary>
        public void UpdateControllers()
        {
            foreach (var controller in controllers)
            {
                controller.Update();
            }
        }

        /// <summary>
        /// Calls the LateUpdate() method of all controllers
        /// </summary>
        public void LateUpdateControllers()
        {
            foreach (var controller in controllers)
            {
                controller.LateUpdate();
            }
        }

          /// <summary>
        /// Calls the Update() method of all controllers
        /// </summary>
        public void FixedUpdateControllers()
        {
            foreach (var controller in controllers)
            {
                controller.FixedUpdate();
            }
        }

    }
}