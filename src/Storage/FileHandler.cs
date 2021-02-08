using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using IMDG.Common;

namespace IMDG.Storage
{
    public class FileHandler
    {
        private readonly string _fileName;

        public FileHandler()
        {
            _fileName = GetFileName();
        }


        public Task UpdateFileAsync(IEnumerable<Command> commands) => FileHelper.TruncateAsync(_fileName, commands);


        public Task<IEnumerable<Command>> GetFile() => FileHelper.ReadCommandsFromFile(_fileName);

        private static string GetFileName()
        {
            var id = Process.GetCurrentProcess().Id;
            return $"StorageFile_{id}.txt";
        }
    }
}