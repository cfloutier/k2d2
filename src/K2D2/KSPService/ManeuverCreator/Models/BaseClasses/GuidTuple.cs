using System;

namespace K2D2.sources.Models.BaseClasses
{
    public class GuidTuple<T>
    {
        public T content { set; get; }
        public Guid guid { get; }
        
        public GuidTuple(T Content, Guid guid)
        {
            this.content = Content;
            this.guid = guid;
        }
    }

}