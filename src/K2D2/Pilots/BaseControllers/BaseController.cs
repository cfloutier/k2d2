
using K2D2.UI;
using KSP.Game;
using UnityEngine.UIElements;

namespace K2D2.Controller
{
    public class BaseController 
    {
        //public List<KSP.Game.GameState> applicableStates = new List<KSP.Game.GameState>();

        //private bool _controllerCurrentlyActive = false;

        public GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

        public bool debug_mode_only = true;
        

   

        bool _enabled = true;

        // is the pilot enabled : for instance it can be hidden because not available yet 
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        bool _is_running = false;

        public virtual bool isRunning
        {
            get {return _is_running;}
            set { 
                if (_is_running == value) return;
                _is_running = value; 
                is_running_event?.Invoke(_is_running);
            }
        }

        public delegate void onRunning(bool is_running);
        public event onRunning is_running_event;

        public void listenIsRunning(onRunning fct)
        {
            is_running_event+=fct;
            fct(isRunning);
        }

        public virtual bool isActive
        {
            get {

                if (!Enabled)
                    return false;
               
                if (debug_mode_only && !K2D2Settings.debug_mode.V)
                    return false;

                return true; 
            } 
        }


        public virtual void onReset()
        {
            // onReset is called each time scene become Invalid, or when a controller need exclusivity
        }

        public virtual void updateUI(VisualElement root_el, FullStatus st)
        {
            // called to update a simple Controler UI
        }

        public virtual void Update()
        {
            // Update is called each frame
        }

        public virtual void LateUpdate()
        {
            // Late Update is called just before rendering
        }

        public virtual void FixedUpdate()
        {
            // Fixed Update is called on physic update
        }


        /*
        /// <summary>
        /// Implement this method to add custom reinitialization code e.g. to reinitialize the vessel after a scene change
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Reinitialize()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Checks if the controller is active and calls Reinitialize() if it needs to be reinitialized
        /// Override Reinitalize() to add custom reinitialization code e.g. to reinitialize the vessel after a scene change
        /// The applicableStates list is used to determine if the controller is active
        /// </summary>
        /// <returns></returns>
        public bool isControllerActive()
        {
            bool activeGameStates = applicableStates.Contains(Game.GlobalGameState.GetState());
            if (_controllerCurrentlyActive && activeGameStates)
            {
                return true;
            }

            if (!_controllerCurrentlyActive && activeGameStates)
            {
                Reinitialize();
                _controllerCurrentlyActive = true;
                return true;
            }

            _controllerCurrentlyActive = false;
            return false;
        }*/

    }

}


