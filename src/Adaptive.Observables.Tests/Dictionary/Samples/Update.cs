using System.Collections;
using System.Collections.Generic;

namespace Adaptive.Observables.Tests.Samples
{
    public abstract class Update : IEnumerable<KeyValuePair<string, string>>
    {
        public Dictionary<string, string> Values { get; set; }
        
        protected Update(string key)
        {
            Values = new Dictionary<string, string>();
            
            Values["key"] = key;
        }

        public void Add(string key, string value)
        {
            Values.Add(key, value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}