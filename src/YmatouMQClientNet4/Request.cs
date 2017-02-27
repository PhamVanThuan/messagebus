using System;
using System.Collections.Generic;

namespace YmatouMessageBusClientNet4
{
    internal class Request
    {
        private readonly IDictionary<string, object> dto;
        public static Request Builder
        {
            get { return new Request(); }
        }
        private Request()
        {
            dto = new Dictionary<string, object>();
        }
        public Request Add(string key, object val)
        {
            dto[key] = val;
            return this;
        }
        public Request Add(Func<bool> condition, string key, object val) 
        {
            if (!condition()) return this;
            dto[key] = val;
            return this;
        }
        public IDictionary<string, object> ToRequestDto() { return this.dto; }
    }
}
