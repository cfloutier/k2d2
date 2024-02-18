using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace K2UI
{
    class Console: Label
    {
        public new class UxmlFactory : UxmlFactory<Console, UxmlTraits> { }

        public new class UxmlTraits : TextElement.UxmlTraits
        {

        }  

        public static new readonly string ussClassName = "console";

        public Console() : base()
        {
            AddToClassList(ussClassName);
        }
    }

}