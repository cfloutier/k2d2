
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace K2UI.Compas
{
    public class ButtonsFactory
    {

        
        string uss_label = "big_text";
        string uss_class = "label_button";

        public Dictionary<int, Button> buttons = new();

        public void start()
        {
            foreach (var label in buttons.Values)
            {
                label.style.display = DisplayStyle.None;
            }
        }

        public Button buttonPool(int angle, K2Compass compass, string text, Vector2 pos)
        {
            Button button = null;

            if (!buttons.ContainsKey(angle))
            {
                // need a new Label
                button = new Button();
                button.RegisterCallback<ClickEvent>(evt =>
                    {
                        compass.forceValue(angle);
                    }
                );     

                button.AddToClassList(uss_label);
                button.AddToClassList(uss_class);

                buttons[angle] = button;
            }
            else
            {
                button = buttons[angle];
            }

            button.style.display = DisplayStyle.Flex;
            // width of the button is hardcoded for the moment
            button.style.left = pos.x - 20;
            button.style.bottom = pos.y;
            button.text = text;
            button.name = text;
            return button;
        }
    }

    // class used to pool a set of Label to avoitd creating new
    public class LabelsFactory
    {
        string uss_big_text = "big_text";
        string uss_small_text = "small_text";

        public List<Label> labels = new();

        int nb_used = 0;

        public void start()
        {
            foreach (var label in labels)
            {
                label.style.display = DisplayStyle.None;
            }
            nb_used = 0;
        }

        public Label labelPool(bool big, string text, Vector2 pos)
        {
            Label label = null;
            if (nb_used >= labels.Count)
            {
                // need a new Label
                label = new Label();
                labels.Add(label);
            }
            else
            {
                label = labels[nb_used];
            }

            nb_used++;

            if (big)
            {
                label.RemoveFromClassList(uss_small_text);
                label.AddToClassList(uss_big_text);
            }
            else
            {
                label.RemoveFromClassList(uss_big_text);
                label.AddToClassList(uss_small_text);
            }

            label.style.display = DisplayStyle.Flex;
            label.style.left = pos.x;
            label.style.bottom = pos.y;
            label.text = text;
            label.name = text;

            return label;
        }

    }
}