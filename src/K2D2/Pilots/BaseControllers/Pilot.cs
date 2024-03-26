using System.Diagnostics.Tracing;
using K2UI.Tabs;
using KSP.UI;


namespace K2D2.Controller
{

    // a pilot is a complex controller that can have ui page
    public class Pilot : ComplexController
    {
        public override bool isRunning
        {
            get => base.isRunning;

            set
            {
                if (value == base.isRunning)
                    return;

                base.isRunning = value;
                // update the panel
                _panel.isRunning = isRunning;
            }
        }

        // MUST be build in constructor
        protected K2Page _panel = null;

        public virtual K2Page page
        {
            get => _panel;
        }
    }

}