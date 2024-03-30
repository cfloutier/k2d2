// using System;
// using System.Collections.Generic;
// using System.Linq;
using K2D2.UI;
using KSP.Game;
using K2D2.UI;
// using KSP.Messages;
using UnityEngine.UIElements;


namespace K2D2.Controller
{
    public class BaseController 
    {
        //public List<KSP.Game.GameState> applicableStates = new List<KSP.Game.GameState>();

        //private bool _controllerCurrentlyActive = false;

        public GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

        // set my the main UI, update can be ignore if not active and not visible
        public bool ui_visible = false;

        public bool debug_mode_only = true;
        public string name = "Unamed";

        public string Name
        {
            get
            {
                return name;
            }
        }

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

        public bool need_update
        {
            get { return ui_visible || isRunning; }
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

        public bool UIVisible { get { return ui_visible; } set { ui_visible = value; } }

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


