using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using IMDG.Common;

namespace IMDG.Manager
{
    public static class ManagerMain
    {
        private static async Task Main()
        {
            Console.WriteLine("[Server] Started");
            var ch = Channel.CreateBounded<Command>(new BoundedChannelOptions(1024)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait,
            });
            var manager = new Manager(ch.Writer);
            var fw = new FileWriter(ch.Reader, manager);
            var storageServer = new StorageServer(manager);
            var clientServer = new ClientServer(manager);
            await Task.WhenAny(
                clientServer.Start(), 
                storageServer.Start(),
                fw.ReadMessages());
        }
    }
}