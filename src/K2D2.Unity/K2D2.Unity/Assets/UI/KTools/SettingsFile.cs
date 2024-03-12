using System.Collections.Generic;
using System;
using System.Threading;
using System.IO;

using UnityEngine;
using UnityEngine.UIElements;

// using Newtonsoft.Json;
// using BepInEx.Logging;

namespace KTools
{

    public class SettingsBoolValue
    {
        public string path;

        bool _value;
        public bool Value
        {
            get { return _value;}
            set { 
                    if (value == _value) return;
                
                _value = value;
                foreach(var listener in listeners)
                    listener(_value);          
            }
        }

        public delegate void onChanged(bool value);

        public List< onChanged> listeners = new List<onChanged>();


        public void ListenTo(VisualElement element)
        {
            element.RegisterCallback<ChangeEvent<bool>>( evt => Value = evt.newValue );
        }
    }

    public class SettingsFile
    {
        protected string file_path = "";
        Dictionary<string, string> data = new Dictionary<string, string>();

        public SettingsFile(string file_path)
        {
            this.file_path = file_path;
            Load();
        }

        // public ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("K2D2.SettingsFile");

        protected void Load()
        {

            var previous_culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                this.data = JsonUtility.FromJson<Dictionary<string, string>>(File.ReadAllText(file_path));    
            }
            catch (System.Exception)
            {
                Debug.LogWarning($"error loading {file_path}");
            }

            Thread.CurrentThread.CurrentCulture = previous_culture;
        }

        protected void Save()
        {
            var previous_culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                File.WriteAllText(file_path, JsonUtility.ToJson(data));
            }
            catch (System.Exception)
            {
                Debug.LogError($"error saving {this.file_path}");
            }

            Thread.CurrentThread.CurrentCulture = previous_culture;
        }

        public string GetString(string name, string defaultValue)
        {
            if (data.ContainsKey(name))
                return data[name];

            return defaultValue;
        }

        public void SetString(string name, string value)
        {
            if (data.ContainsKey(name))
            {
                if (data[name] != value)
                {
                    data[name] = value;
                    Save();
                }
            }
            else
            {
                data[name] = value;
                Save();
            }
        }


        /// <summary>
        /// Get the parameter using bool value
        /// if not found it is added and saved at once
        /// </summary>
        public bool GetBool(string name, bool defaultValue)
        {
            if (data.ContainsKey(name))
                return data[name] == "1";
            else
                SetBool(name, defaultValue);

            return defaultValue;
        }

        /// <summary>
        /// Set the parameter using bool value
        /// the value is saved at once
        /// </summary>
        public void SetBool(string name, bool value)
        {
            string value_str = value ? "1" : "0";
            SetString(name, value_str);
        }


        /// <summary>
        /// Get the parameter using integer value
        /// if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public int GetInt(string name, int defaultValue)
        {
            if (data.ContainsKey(name))
            {
                int value = 0;
                if (int.TryParse(data[name], out value))
                {
                    return value;
                }
            }

            // invalid or no value found in data
            SetInt(name, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Set the parameter using integer value
        /// the value is saved at once
        /// </summary>
        public void SetInt(string name, int value)
        {
            SetString(name, value.ToString());
        }

        public TEnum GetEnum<TEnum>(string name, TEnum defaultValue) where TEnum : struct
        {
            if (data.ContainsKey(name))
            {
                TEnum value = defaultValue;
                if (Enum.TryParse<TEnum>(data[name], out value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        public void SetEnum<TEnum>(string name, TEnum value) where TEnum : struct
        {
            SetString(name, value.ToString());
        }


        /// <summary>
        /// Get the parameter using float value
        /// if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public float GetFloat(string name, float defaultValue)
        {
            if (data.ContainsKey(name))
            {
                float value = 0;
                if (float.TryParse(data[name], out value))
                {
                    return value;
                }
            }

            // invalid or no value found in data
            SetFloat(name, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Set the parameter using float value
        /// the value is saved at once
        /// </summary>
        public void SetFloat(string name, float value)
        {
            SetString(name, value.ToString());
        }

        /// <summary>
        /// Get the parameter using double value
        /// if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public double GetDouble(string name, double defaultValue)
        {
            if (data.ContainsKey(name))
            {
                double value = 0;
                if (double.TryParse(data[name], out value))
                {
                    return value;
                }
            }

            // invalid or no value found in data
            SetDouble(name, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Set the parameter using double value
        /// the value is saved at once
        /// </summary>
        public void SetDouble(string name, double value)
        {
            SetString(name, value.ToString());
        }

        /// <summary>
        /// Get the parameter using Vector3 value
        ///  if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public Vector3 GetVector3(string name, Vector3 defaultValue)
        {
            if (!data.ContainsKey(name))
            {
                SetVector3(name, defaultValue);
                return defaultValue;
            }

            string txt = (string)data[name];
            string[] ar = txt.Split(';');

            if (ar.Length < 3)
            {
                SetVector3(name, defaultValue);
                return defaultValue;
            }

            Vector3 result = Vector3.zero;
            try
            {
                result.x = float.Parse(ar[0]);
                result.y = float.Parse(ar[1]);
                result.z = float.Parse(ar[2]);
            }
            catch
            {
                SetVector3(name, defaultValue);
                return defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Set the parameter using Vector3 value
        /// the value is saved at once
        /// </summary>
        public void SetVector3(string name, Vector3 value)
        {
            string text = value.x + ";" + value.y + ";" + value.z;
            SetString(name, text);
        }

        /// <summary>
        /// Get the parameter using Vector3d value
        ///  if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        // public Vector3 GetVector3d(string name, Vector3d defaultValue)
        // {
        //     if (!data.ContainsKey(name))
        //     {
        //         SetVector3d(name, defaultValue);
        //         return defaultValue;
        //     }

        //     string txt = data[name];
        //     string[] ar = txt.Split(';');

        //     if (ar.Length < 3)
        //     {
        //         SetVector3d(name, defaultValue);
        //         return defaultValue;
        //     }

        //     Vector3d result = Vector3d.zero;
        //     try
        //     {
        //         result.x = double.Parse(ar[0]);
        //         result.y = double.Parse(ar[1]);
        //         result.z = double.Parse(ar[2]);
        //     }
        //     catch
        //     {
        //         SetVector3d(name, defaultValue);
        //         return defaultValue;
        //     }

        //     return result;
        // }

        // public void SetVector3d(string name, Vector3d value)
        // {
        //     string text = value.x + ";" + value.y + ";" + value.z;
        //     SetString(name, text);
        // }

        public Color GetColor(string name, Color defaultValue)
        {
            if (!data.ContainsKey(name))
            {
                SetColor(name, defaultValue);
                return defaultValue;
            }

            string txt = data[name];
            Color result = ColorTools.parseColor(txt);
            return result;
        }

        public void SetColor(string name, Color value)
        {
            string text = ColorTools.formatColorHtml(value);
            SetString(name, text);
        }

    }

}