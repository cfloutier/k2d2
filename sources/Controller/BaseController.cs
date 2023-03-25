using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.Messages;

namespace K2D2.Controller
{
    public class BaseController
    {
        public List<ButtonBase> buttons = new List<ButtonBase>();

        public List<KSP.Game.GameState> applicableStates = new List<KSP.Game.GameState>();
        
        private bool _controllerCurrentlyActive = false;
        
        public GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

        private protected void Run()
        {
            foreach (var button in buttons.Where(button => button.active))
            {
                button.Run();
            }
        }
        
        public virtual void onGUI()
        {
            
            throw new System.NotImplementedException();
        }
        
        public virtual void Update()
        {
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

    public delegate void Action();
    public class ButtonBase
    {
        public bool active = false;

        public Action action;
        
        public void _switch()
        {
            this.active = !active;
        }

        public virtual void Run()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class Switch : ButtonBase
    {
        public override void Run()
        {
            action(); 
        }
    }

    public class Button : ButtonBase
    {
        
        public override void Run()
        {
            action();
            active = !active;
        }
    }
    
    
}


