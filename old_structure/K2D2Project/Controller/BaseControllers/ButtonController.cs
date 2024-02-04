﻿using System;
using System.Collections.Generic;
using System.Linq;
using KSP.Game;
using KSP.Messages;

namespace K2D2.Controller
{

   public class ButtonController : BaseController
    {
        public List<ButtonBase> buttons = new List<ButtonBase>();

        private protected void Run()
        {
            foreach (var button in buttons.Where(button => button.active))
            {
                button.Run();
            }
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


