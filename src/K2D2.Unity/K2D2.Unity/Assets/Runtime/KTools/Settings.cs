using System.Collections.Generic;
using System;
using System.Threading;
using System.IO;

using UnityEngine;
using UnityEngine.UIElements;

using Newtonsoft.Json;
using K2UI;
using System.Globalization;

namespace KTools
{
    public interface IResettable
    {
        void Reset(string path);
    }

    public class EnumSetting <TEnum> : IResettable where TEnum : struct
    {
        public string path;
        TEnum default_value;
        public EnumSetting(string path, TEnum default_value)
        {
            this.path = path;
            this.default_value = default_value;
            if (SettingsFile.Instance.loaded)
                loadValue();
            else
                SettingsFile.Instance.onloaded_event += loadValue;

            if (!string.IsNullOrEmpty(path))    
                SettingsFile.Instance.reset_register.Add(this);
        }

        public void Reset(string path = null)
        {
            if (path != null)
                if (!this.path.StartsWith(path))
                    return;
                
            this.V = default_value;
        }

        void loadValue()
        {
            _value = SettingsFile.Instance.GetEnum<TEnum>(path, default_value);
        }

        TEnum _value;
        public TEnum V
        {
            get { return _value; }
            set
            {
                if (value.Equals(_value)) return;

                _value = value;
                listeners?.Invoke(this.V);

                SettingsFile.Instance.SetEnum<TEnum>(path, _value);
            }
        }

        public int int_value
        {
            get { return (int)(object) V;}
            set { _value = (TEnum)(object) value;}
        }

        public delegate void onChanged(TEnum value);

        public event onChanged listeners;
    }

    public class Setting<T> : IResettable
    {
        public string key;
        T default_value;

        public Setting(string path, T default_value)
        {
            this.key = path;
            this.default_value = default_value; 
            if (SettingsFile.Instance.loaded)
                loadValue();
            else
                SettingsFile.Instance.onloaded_event += loadValue;

            if (!string.IsNullOrEmpty(path))    
                SettingsFile.Instance.reset_register.Add(this);
        }

        public void Reset(string path = null)
        {   
            if (path != null)
                if (!this.key.StartsWith(path))
                    return;
                    
            this.V = default_value;
        }

        void loadValue()
        {
            _value = SettingsFile.Instance.Get<T>(key, default_value);
            // if (path == "lift.end_ascent_pc")
            //     Debug.Log("load value" + _value);
        }

        T _value;
        public virtual T V
        {
            get { return _value; }
            set
            {
                if (value.Equals(_value))
                    return;

                if (!string.IsNullOrEmpty(key))    
                    if (!SettingsFile.Instance.Set<T>(key, value))
                        return;

                _value = value;
                // if (path == "lift.end_ascent_pc")
                //     Debug.Log("set value" + _value);
                listeners?.Invoke(this.V); 
            }
        }

        public delegate void onChanged(T value);

        public event onChanged listeners;

        public void listen(onChanged listener)
        {
            listeners+= listener;
            listener(V);
        }
    }


    public class ClampSetting<T> : Setting<T> where T : System.IComparable<T>
    {
        T _min;
        public T min
        {
            get { return _min; }
            set { 
                _min = value;
                V = Extensions.Clamp(value, min, max);
            }
        }

        T _max;
        public T max
        {
            get { return _max; }
            set { 
                _max = value;
                V = Extensions.Clamp(value, min, max);
            }
        }
        
        public ClampSetting(string path, T default_value, T min, T max): base(path, default_value)
        {
            this._min = min;
            this._max = max;  
            V = Extensions.Clamp(V, min, max);
        }

        public override T V { 
            get => base.V; 
            set {
                
                base.V = value;
            } 
        }
    }

    public class ClampedSettingInt : Setting<int>
    {
        
        int _min;
        public int min
        {
            get { return _min; }
            set { 
                _min = value;
                V = Extensions.Clamp(value, min, max);
            }
        }

        int _max;
        public int max
        {
            get { return _max; }
            set { 
                _max = value;
                V = Extensions.Clamp(value, min, max);
            }
        }

        public ClampedSettingInt(string path, int default_value, int min, int max): base(path, default_value)
        {
            this.min = min;
            this.max = max;     
        }

        int clamp(int value)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;

            return value;
        }

        public override int V { 
            get => base.V; 
            set {
                base.V = clamp(value);
            } 
        }
    }



}