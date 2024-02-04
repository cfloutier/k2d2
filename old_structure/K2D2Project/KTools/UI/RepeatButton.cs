
using System.Collections.Generic;
using UnityEngine;

namespace KTools.UI
{
    public class RepeatButton
    {
        class ButtonInstance
        {
            public ButtonInstance(string ui_code)
            {
                this.ui_code = ui_code;
            }

            public string ui_code;
            public bool is_active = false;
            public float next_time;
            public float delta_time = 0;

            const float min_delta_s = 0.001f;

            public float OnGUI(string txt, float value, float delta)
            {
                GUI.SetNextControlName(ui_code);
                bool is_On = GUILayout.RepeatButton(txt, KBaseStyle.repeat_button, GUILayout.Height(22));

                if (Event.current.type == EventType.Repaint)
                {
                    if (is_On)
                    {
                        if (!is_active)
                        {
                            is_active = true;
                            delta_time = start_delta_time;
                            next_time = Time.time + delta_time;
                            value += delta;
                        }
                        else if (Time.time > next_time)
                        {
                            delta_time = delta_time * 0.9f;
                            if (delta_time < min_delta_s)
                                delta_time = min_delta_s;
                            next_time = Time.time + delta_time;

                            value += delta;
                        }
                    }
                    else
                    {
                        is_active = false;
                    }
                }

                return value;
            }
        }

        static Dictionary<string, ButtonInstance> instances = new Dictionary<string, ButtonInstance>();
        static float start_delta_time = 0.3f;

        public static float OnGUI(string instance_name, string txt, float value, float delta)
        {
            ButtonInstance instance = null;
            if (!instances.ContainsKey(instance_name))
            {
                instance = new ButtonInstance(instance_name);
                instances[instance_name] = instance;
            }
            else
                instance = instances[instance_name];

            return instance.OnGUI(txt, value, delta);
        }

        public static double OnGUI(string instance_name, string txt, double value, double delta)
        {
            return (double)OnGUI(instance_name, txt, (float)value, (float)delta);
        }

    }

}