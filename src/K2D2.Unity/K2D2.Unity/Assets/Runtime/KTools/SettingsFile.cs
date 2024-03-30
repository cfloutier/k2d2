using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Newtonsoft.Json;
using K2UI;
using System.Globalization;
using System.Runtime.CompilerServices;


namespace KTools
{
    public class SettingsFile
    {
        static SettingsFile _instance;
        static public SettingsFile Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsFile();

                return _instance;
            }
        }

        static public void Init(MonoBehaviour main, string file_path)
        {
            Instance.Load(main, file_path);
        }

        public List<IResettable> reset_register = new();

        public void Reset(string chapter = null)
        {
            foreach(var s in reset_register)
            {
                s.Reset(chapter);
            }
            needSave = true;
        }
        bool needSave = false;

        protected string file_path = "";
        Dictionary<string, string> data = new Dictionary<string, string>();

        public bool loaded = false;

        public delegate void onLoaded();

        public event onLoaded onloaded_event;

        protected void Load(MonoBehaviour main, string file_path)
        {
            this.file_path = file_path;
            var previous_culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                this.data = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file_path));
            }
            catch (System.Exception)
            {
                Debug.LogWarning($"error loading {file_path}");
            }

            Thread.CurrentThread.CurrentCulture = previous_culture;
            loaded = true;
            onloaded_event?.Invoke();
            main.StartCoroutine(SaveIfNeeded());
        }

        IEnumerator SaveIfNeeded()
        {
            while (Application.isPlaying)
            {
                if (needSave)
                {
                    Save();
                }
                yield return new WaitForSeconds(1);
            }
        }

        protected void Save()
        {
            // Debug.Log("settings saved");
            
            var previous_culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                File.WriteAllText(file_path, JsonConvert.SerializeObject(data));
            }
            catch (System.Exception)
            {
                Debug.LogError($"error saving {this.file_path}");
            }

            Thread.CurrentThread.CurrentCulture = previous_culture;
            needSave = false;
        }

        public T Get<T>(string key, T default_value)
        {
            if (!data.ContainsKey(key))
                return default_value;

            Type object_type = typeof(T);
            if (object_type == null)
            {
                throw new ArgumentNullException();
            }
            else if (object_type == typeof(string))
            {
                return (T)(object)GetString(key, (string)(object)default_value);
            }
            else if (object_type == typeof(bool))
            {
               return (T)(object)GetBool(key, (bool)(object)default_value);
            }
            else if (object_type == typeof(int))
            {
                return (T)(object)GetInt(key, (int)(object)default_value);
            }
            else if (object_type == typeof(float))
            {
                return (T)(object)GetFloat(key, (float)(object)default_value);
            }
            else if (object_type == typeof(double))
            {
                return (T)(object)GetDouble(key, (double)(object)default_value);
            }
            else if (object_type == typeof(Color))
            {
                return (T)(object)GetColor(key, (Color)(object)default_value);
            }
            else if (object_type == typeof(Vector3))
            {
                return (T)(object)GetVector3(key, (Vector3)(object)default_value);
            }
            else
            {
                throw new InvalidCastException(object_type + " not implemented");
            }
        }


        public bool Set<T>(string key, T value)
        {
            Type object_type = typeof(T);
            if (object_type == null)
            {
                throw new ArgumentNullException();
            }
            else if (object_type == typeof(string))
            {
                return SetString(key, (string)(object)value);
            }
            else if (object_type == typeof(bool))
            {
                return SetBool(key, (bool)(object)value);
            }
            else if (object_type == typeof(int))
            {
                return SetInt(key, (int)(object)value);
            }
            else if (object_type == typeof(float))
            {
                return SetFloat(key, (float)(object)value);
            }
            else if (object_type == typeof(double))
            {
                return SetDouble(key, (double)(object)value);
            }
            else if (object_type == typeof(Color))
            {
                return SetColor(key, (Color)(object)value);
            }
            else if (object_type == typeof(Vector3))
            {
                return SetVector3(key, (Vector3)(object)value);
            }
            else
            {
                throw new InvalidCastException(object_type + " not implemented");
            }
        }

        public string GetString(string key, string default_value)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }

            return default_value;
        }

        public bool SetString(string key, string value)
        {
            if (data.ContainsKey(key))
            {
                if (data[key] == value)
                    return false;

                if (data[key] != value)
                {
                    data[key] = value;
                    needSave = true;
                }
                return true;
            }
            else
            {
                data[key] = value;
                needSave = true;
                return true;
            }
        }


        /// <summary>
        /// Get the parameter using bool value
        /// if not found it is added and saved at once
        /// </summary>
        public bool GetBool(string key, bool default_value)
        {
            if (data.ContainsKey(key))
                return data[key] == "1";
            else
                SetBool(key, default_value);

            return default_value;
        }

        /// <summary>
        /// Set the parameter using bool value
        /// the value is saved at once
        /// </summary>
        public bool SetBool(string key, bool value)
        {
            string value_str = value ? "1" : "0";
            return SetString(key, value_str);
        }


        /// <summary>
        /// Get the parameter using integer value
        /// if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public int GetInt(string key, int default_value)
        {
            
            if (data.ContainsKey(key))
            {
                int value = 0;
                if (int.TryParse(data[key], out value))
                {
                    return value;
                }
            }

            // invalid or no value found in data
            SetInt(key, default_value);
            return default_value;
        }

        /// <summary>
        /// Set the parameter using integer value
        /// the value is saved at once
        /// </summary>
        public bool SetInt(string key, int value)
        {
            return SetString(key, value.ToString());
        }

        public TEnum GetEnum<TEnum>(string key, TEnum default_value) where TEnum : struct
        {
            if (data.ContainsKey(key))
            {
                TEnum value = default_value;
                if (Enum.TryParse<TEnum>(data[key], out value))
                {
                    return value;
                }
            }

            return default_value;
        }

        public bool SetEnum<TEnum>(string key, TEnum value) where TEnum : struct
        {
            return SetString(key, value.ToString());
        }


        /// <summary>
        /// Get the parameter using float value
        /// if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public float GetFloat(string key, float default_value)
        {  
            if (data.ContainsKey(key))
            {
                float value = 0;
                if (float.TryParse(data[key], NumberStyles.Float | NumberStyles.AllowThousands,
                             CultureInfo.InvariantCulture, out value))
                {
                    return value;
                }
            }

            // invalid or no value found in data
            SetFloat(key, default_value);
            
            return default_value;
        }

        /// <summary>
        /// Set the parameter using float value
        /// the value is saved at once
        /// </summary>
        public bool SetFloat(string key, float value)
        {
           return SetString(key, value.ToStringInvariant());         
        }

        /// <summary>
        /// Get the parameter using double value
        /// if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public double GetDouble(string key, double default_value)
        {
            if (data.ContainsKey(key))
            {
                double value = 0;
                if (double.TryParse(data[key], NumberStyles.Float | NumberStyles.AllowThousands,
                             CultureInfo.InvariantCulture, out value))
                {
                    return value;
                }
            }

            // invalid or no value found in data
            SetDouble(key, default_value);
            return default_value;
        }

        /// <summary>
        /// Set the parameter using double value
        /// the value is saved at once
        /// </summary>
        public bool SetDouble(string key, double value)
        {           
            return SetString(key, value.ToStringInvariant());
        }

        /// <summary>
        /// Get the parameter using Vector3 value
        ///  if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        public Vector3 GetVector3(string key, Vector3 default_value)
        {
            if (!data.ContainsKey(key))
            {
                SetVector3(key, default_value);
                return default_value;
            }

            string txt = (string)data[key];
            string[] ar = txt.Split(';');

            if (ar.Length < 3)
            {
                SetVector3(key, default_value);
                return default_value;
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
                SetVector3(key, default_value);
                return default_value;
            }

            return result;
        }

        /// <summary>
        /// Set the parameter using Vector3 value
        /// the value is saved at once
        /// </summary>
        public bool SetVector3(string key, Vector3 value)
        {
            string text = value.x + ";" + value.y + ";" + value.z;
            return SetString(key, text);
        }

        /// <summary>
        /// Get the parameter using Vector3d value
        ///  if not found or on parsing error, it is replaced and saved at once
        /// </summary>
        // public Vector3 GetVector3d(string key, Vector3d default_value)
        // {
        //     if (!data.ContainsKey(key))
        //     {
        //         SetVector3d(key, default_value);
        //         return default_value;
        //     }

        //     string txt = data[key];
        //     string[] ar = txt.Split(';');

        //     if (ar.Length < 3)
        //     {
        //         SetVector3d(key, default_value);
        //         return default_value;
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
        //         SetVector3d(key, default_value);
        //         return default_value;
        //     }

        //     return result;
        // }

        // public void SetVector3d(string key, Vector3d value)
        // {
        //     string text = value.x + ";" + value.y + ";" + value.z;
        //     SetString(key, text);
        // }

        public Color GetColor(string key, Color default_value)
        {
            if (!data.ContainsKey(key))
            {
                SetColor(key, default_value);
                return default_value;
            }

            string txt = data[key];
            Color result = ColorTools.parseColor(txt);
            return result;
        }

        public bool SetColor(string key, Color value)
        {
            string text = ColorTools.formatColorHtml(value);
            return SetString(key, text);
        }

    }

}