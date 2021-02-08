using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IMDG.Common
{
    public static class FileHelper
    {
        public static async Task<IEnumerable<Command>> ReadCommandsFromFile(string fileName, Encoding encoding = null)
        {
            if (!File.Exists(fileName)) return Enumerable.Empty<Command>();
            var lines = await File.ReadAllLinesAsync(fileName, encoding ?? Encoding.UTF8);
            return lines
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.TrimEnd())
                .Select(l =>
                    JsonConvert.DeserializeObject<Command>(l, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    }));
        }

        public static async Task TruncateAsync(string fileName, IEnumerable<Command> commands, Encoding encoding = null)
        {
            var lines = commands.Select(c => JsonConvert.SerializeObject(c, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            }));
            await File.WriteAllLinesAsync(fileName, lines, encoding ?? Encoding.UTF8);
        }
    }
}