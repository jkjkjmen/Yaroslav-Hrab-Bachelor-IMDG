using System;
using IMDG.Common;
using Newtonsoft.Json;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new JsonSerializerSettings
            {
                //TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
            };
            var command = new GetCommand("1");
            var s = JsonConvert.SerializeObject(command, Formatting.Indented, settings);
            Console.WriteLine(s);
            var deserializeObject = JsonConvert.DeserializeObject<Command>(s, settings);
        }
    }
}