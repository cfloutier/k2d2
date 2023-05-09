
using System.Collections.Generic;
using UnityEngine;

namespace KTools.UI;

public class RepeatButton
{
    class ButtonInstance
    {
        public ButtonInstance(string name)
        {
            this.name = name;
        }

        public string name;
        public bool is_active = false;
        public float next_time;
        public float delta_time = 0;

        public float OnGUI(string txt, float value, float delta)
        {
            bool is_On = GUILayout.RepeatButton(txt, KBaseStyle.small_button, GUILayout.Width(20), GUILayout.Height(22));

            if (Event.current.type == EventType.Repaint)
            {
                if (is_On)
                {
                    if (!is_active)
                    {
                        is_active = true;
                        delta_time = start_delta_time;
                        next_time = Time.time + delta_time;
                        Debug.Log("value  " + value);
                        value += delta;

                        Debug.Log("value  " + value);
                        Debug.Log("delta  " + delta);
                    }
                    else if (Time.time > next_time)
                    {
                        delta_time = delta_time * 0.9f;
                        if (delta_time < 0.1f)
                            delta_time = 0.1f;
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

    public static float Draw(string instance_name, string txt, float value, float delta)
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

    public static double Draw(string instance_name, string txt, double value, double delta)
    {
        return (double)Draw(instance_name, txt, (float)value, (float)delta);
    }

}

