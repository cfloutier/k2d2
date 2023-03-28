using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.Messages;

namespace K2D2.Controller
{
    public class BaseController
    {
        public List<KSP.Game.GameState> applicableStates = new List<KSP.Game.GameState>();

        private bool _controllerCurrentlyActive = false;

        public GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

        public virtual void onReset()
        {
            // onReset is called each time scene become Invalid, or when a controller need exclusivity
        }

        public virtual void onGUI()
        {
            // On GUI is used to draw UI in needed, using GUILayout
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
        }

    }

}


