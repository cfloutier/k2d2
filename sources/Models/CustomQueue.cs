using System;
using System.Collections.Generic;
using K2D2.sources.Models;
using KSP.UI.Binding.Widget;

namespace K2D2
{
    public class FunctionObject : IDescription, IRunnable
    {
        private Action<Delegate, object[]> functionWrapper;
        private object[] parameters;
        Delegate _innerFunction;
        public string description { get; set; }

        public FunctionObject(Delegate innerFunction, params object[] parameters)//Action<Delegate, object[]>
        {
            this._innerFunction = innerFunction;
            this.functionWrapper = Wrapper.FunctionWrapper;
            this.parameters = parameters;
            this.description = "No description";
        }
        
        public void Run()
        {
            functionWrapper(_innerFunction, parameters);
        }
        
        public string GetDescription()
        {
            return description;
        }
        
        public void SetDescription(string description)
        {
            this.description = description;
        }
    }
    
    public class Element<T>
    {
        /*private Action<Delegate, object[]> func;
        private object[] parameters;
        public Delegate innerfunc;*/
        public T content { set; get; }
        public Element<T> next;
        
        public Element(T Content)//Action<Delegate, object[]> func, Delegate innerfunc, params object[] parameters
        {
            this.content = Content;
        }
    }

    public class CustomQueue<T>
    {
        internal Element<T> _head;

        public CustomQueue()
        {
        }

        public void Add(T content)
        {
            Element<T> cache = _head;
            if (_head == null)
            {
                _head = new Element<T>(content);
                return;
            }

            while (cache.next != null)
            {
                cache = cache.next;
            }

            cache.next = new Element<T>(content);
        }

        public void Pop()
        {
            if (_head == null)
                throw new Exception("Queue is empty");
            _head = _head.next;
        }

        public bool HasNext()
        {
            if (_head == null)
                return false;
            return true;
        }

        public void Clear()
        {
            _head = null;
        }

    }
    
    public class FunctionQueue : CustomQueue<FunctionObject>{
        
        
        public void Add(Delegate innerFunction, params object[] parameters)
        {
            FunctionObject functionObject = new FunctionObject(innerFunction, parameters);
            Add(functionObject);
        }
        
        public void Add(string Description, Delegate innerFunction, params object[] parameters)
        {
            FunctionObject functionObject = new FunctionObject(innerFunction, parameters);
            functionObject.SetDescription(Description);
            Add(functionObject);
        }
        
        private void Add(FunctionObject functionObject)
        {
            if (_head == null)
            {
                _head = new Element<FunctionObject>(functionObject);
                return;
            }
            
            Element<FunctionObject> cache = _head;
            
            while (cache.next != null)
            {
                cache = cache.next;
            }

            cache.next = new Element<FunctionObject>(functionObject);
        }

        public string PopAndRun()
        {
            string description ="No description";
            if (_head == null)
                throw new Exception("Queue is empty");
            
            if(_head.content is IDescription)
                description = _head.content.GetDescription();
            _head.content.Run();
            _head = _head.next;
            return description;
        }
        
        public List<string> ViewQueue()
        {
            Element<FunctionObject> cache = _head;
            List<string> descriptions = new List<string>();
            while (cache != null)
            {
                if(cache.content is IDescription)
                    descriptions.Add(cache.content.GetDescription());
                cache = cache.next;
            }
            return descriptions;
        }
        
    }

    class Wrapper
    {
        public static void FunctionWrapper(Delegate func, params object[] parameters)
        {
            // Call the function with the provided parameters
            func.DynamicInvoke(parameters);
        }
    }
}