using System;
using Newtonsoft.Json;

namespace Valkyrie.Model
{
    public abstract class GenericPort<T> : CemPort
    {
        [JsonIgnore]
        public override Type Type
        {
            get => typeof(T);
            set => throw new Exception();
        }
    }
}