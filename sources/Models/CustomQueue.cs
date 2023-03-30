using System;
using System.Collections.Generic;
using K2D2.sources.Models;
using K2D2.sources.Models.BaseClasses;
using KSP.UI.Binding.Widget;

namespace K2D2
{
    public class FunctionObject : IDescription, IRunnable
    {
        private Action<Delegate, object[]> functionWrapper;
        private object[] parameters;
        Delegate _innerFunction;
        public string description { get; set; }
        public DetailsObject detailsObject { get; set; }

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
        
        public void SetDetailsObject(DetailsObject detailsObject)
        {
            this.detailsObject = detailsObject;
        }
        
        public DetailsObject GetDetailsObject()
        {
            return detailsObject;
        }
    }
    
    public class Element<T>
    {
        /*private Action<Delegate, object[]> func;
        private object[] parameters;
        public Delegate innerfunc;*/
        public T content { set; get; }
        public Element<T> next;
        public Guid guid { get; }
        
        public Element(T Content)//Action<Delegate, object[]> func, Delegate innerfunc, params object[] parameters
        {
            this.content = Content;
            this.guid = Guid.NewGuid();
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
        
        public void Add(string description, Delegate innerFunction, params object[] parameters)
        {
            FunctionObject functionObject = new FunctionObject(innerFunction, parameters);
            functionObject.SetDescription(description);
            Add(functionObject);
        }
        
        public void Add(string description,DetailsObject detailsObject, Delegate innerFunction, params object[] parameters)
        {
            FunctionObject functionObject = new FunctionObject(innerFunction, parameters);
            functionObject.SetDescription(description);
            functionObject.SetDetailsObject(detailsObject);
            Add(functionObject);
        }

        /*public void Add(DetailsObject detailsObject, Delegate innerFunction, params object[] parameters)
        {
            FunctionObject functionObject = new FunctionObject(innerFunction, parameters);
            functionObject.SetDetailsObject(detailsObject);
            Add(functionObject);
        }*/
        
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
        
        public List<FunctionObject> ToList()
        {
            Element<FunctionObject> cache = _head;
            List<FunctionObject> maneuvers = new List<FunctionObject>();
            while (cache != null)
            {
                maneuvers.Add(cache.content);
                cache = cache.next;
            }
            return maneuvers;
        }

        public List<GuidTuple<FunctionObject>> ToGuidList()
        {
            Element<FunctionObject> cache = _head;
            List<GuidTuple<FunctionObject>> maneuvers = new List<GuidTuple<FunctionObject>>();
            while (cache != null)
            {
                maneuvers.Add(new GuidTuple<FunctionObject>( cache.content,cache.guid));
                cache = cache.next;
            }
            return maneuvers;
        }

        

        public void Clear()
        {
            _head = null;
        }
        
        public void RemoveElement(Guid guid)
        {
            Element<FunctionObject> cache = _head;
            if (cache.guid == guid)
            {
                _head = _head.next;
                return;
            }
            while (cache.next != null)
            {
                if (cache.next.guid == guid)
                {
                    cache.next = cache.next.next;
                    return;
                }
                cache = cache.next;
            }
        }
        
        public void RemoveElementAndAllAfter(Guid guid)
        {
            Element<FunctionObject> cache = _head;
            if (cache.guid == guid)
            {
                _head = null;
                return;
            }
            while (cache.next != null)
            {
                if (cache.next.guid == guid)
                {
                    cache.next = null;
                    return;
                }
                cache = cache.next;
            }
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