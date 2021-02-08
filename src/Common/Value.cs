using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace IMDG.Common
{
    public abstract class Value
    {
        public abstract string Display();
    }


    public class StringValue : Value
    {
        public string Value { get; set; }
        
        public StringValue(string value)
        {
            Value = value;
        }

        public override string Display() => Value;
    }

    public class HashValue : Value
    {
        public ConcurrentDictionary<string, string> Value { get; set; } = new ConcurrentDictionary<string, string>();

        public override string Display() =>
            Value.Aggregate(string.Empty,
                (current, keyValue) => current + $"\n({keyValue.Key}" + " " + $"{keyValue.Value})");
        public string GetValues() =>
            Value.Aggregate(string.Empty,
                (current, keyValue) => current + $"\n({keyValue.Value})");
        public string GetKeys() =>
            Value.Aggregate(string.Empty,
                (current, keyValue) => current + $"\n({keyValue.Key})");
    }

    public class ListValue : Value
    {
        public List<string> Value { get; set; } = new List<string>();

        public override string Display() =>
            Value.Aggregate(string.Empty,(current,value)=> current + $"{value}" + " ");
    }
}