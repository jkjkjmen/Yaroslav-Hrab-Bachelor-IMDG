using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using IMDG.Common;
using IMDG.Parser;
using Newtonsoft.Json;

namespace IMDG.Manager
{
    public class FileWriter
    {
        public const string FileName = "file.json";
        
        private readonly ChannelReader<Command> _rx;
        private readonly Manager _manager;

        private Timer _timer;

        public FileWriter(ChannelReader<Command> rx, Manager manager)
        {
            _rx = rx;
            _manager = manager;
            _timer = new Timer(_ => FileSync(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        }

        public Task ReadMessages()
        {
            return Task.Run(async () =>
            {
                await foreach (var command in _rx.ReadAllAsync())
                {
                    var line = JsonConvert.SerializeObject(command, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    });
                    await File.AppendAllLinesAsync(FileName,new[] {line }, Encoding.UTF8);
                }
            });
        }

        public async void FileSync()
        {
            var commands = await FileHelper.ReadCommandsFromFile(FileName);
            await _manager.SendSync(commands);
        }
        
    }
}