using System.Collections.Generic;
using System;
using System.Threading;
using System.IO;

using UnityEngine;
using UnityEngine.UIElements;

using Newtonsoft.Json;
using K2UI;
using System.Globalization;
using JetBrains.Annotations;


namespace KTools
{
    public interface IResettable
    {
        void Reset();
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

        }

        public void Reset()
        {
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
        }

        public delegate void onChanged(TEnum value);

        public event onChanged listeners;

        public void Bind(InlineEnum element, string labels = null)
        {    
            if (labels == null)
                element.labels = String.Join(";", Enum.GetNames( typeof(TEnum) ));
            else
                element.labels = labels;

            element.RegisterCallback<ChangeEvent<int>>(evt => V =(TEnum)Enum.ToObject(typeof(TEnum), evt.newValue));
        }
    }

    public class Setting<T> : IResettable
    {
        public string path;
        T default_value;

        public Setting(string path, T default_value)
        {
            this.path = path;
            this.default_value = default_value; 
            if (SettingsFile.Instance.loaded)
                loadValue();
            else
                SettingsFile.Instance.onloaded_event += loadValue;
        }

        public void Reset()
        {
            this.V = default_value;
        }

        void loadValue()
        {
            _value = SettingsFile.Instance.Get<T>(path, default_value);
        }

        T _value;
        public virtual T V
        {
            get { return _value; }
            set
            {
                if (value.Equals(_value)) return;

                _value = value;
                listeners?.Invoke(this.V);

                SettingsFile.Instance.Set<T>(path, _value);
            }
        }

        public delegate void onChanged(T value);

        public event onChanged listeners;
    }


    public class ClampSetting<T> : Setting<T> where T : System.IComparable<T>
    {
        public T min, max;

        public ClampSetting(string path, T default_value, T min, T max): base(path, default_value)
        {
            this.min = min;
            this.max = max;     
        }


        public override T V { 
            get => base.V; 
            set {
                value = Extensions.Clamp(value, min, max);
                base.V = value;
            } 
        }
    }

    public class ClampedSettingInt : Setting<int>
    {
        int min, max;

        public ClampedSettingInt(string path, int default_value, int min, int max): base(path, default_value)
        {
            this.min = min;
            this.max = max;     
        }


        public override int V { 
            get => base.V; 
            set {
                if (value < min)
                    value = min;
                else if (value > max)
                    value = max;


                value = Mathf.Clamp(value, min, max);
                base.V = value;
            } 
        }
    }



}