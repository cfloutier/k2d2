using K2UI;
using UnityEngine.UIElements;

namespace K2D2.Controller
{

    public class SingleExecuteController : BaseController
    {
        public void setController(ExecuteController controler)
        {
            sub_controler = controler;
        }

        public ExecuteController sub_controler;


        public bool finished
        {
            get
            {
                if (sub_controler == null)
                    return false;

                return sub_controler.finished;
            }
        }

        public string status_line
        {
            get
            {
                if (sub_controler == null)
                    return "";

                return sub_controler.status_line;
            }
        }

        public override void updateUI(VisualElement root_el, FullStatus st) { if (sub_controler != null) sub_controler.updateUI(root_el, st); }
        public override void Update() { if (sub_controler != null) sub_controler.Update(); }
        public override void LateUpdate() { if (sub_controler != null) sub_controler.LateUpdate(); }
        public override void FixedUpdate() { if (sub_controler != null) sub_controler.FixedUpdate(); }
    }

    /// base class for all Execute controller
    /// each Execute controller have a specific task like Auto warp or Auto Burn to achieve.
    /// * Start is called to init the pilot
    /// * Must implement some of the Update,LateUpdate...
    /// must set the finished when task is accomplished
    public class ExecuteController : ComplexController
    {
        public bool finished = false;
        public string status_line = "";

        // called everytime the Pilot shoudl start
        public virtual void Start()
        {
            finished = false;
        }
    }

}