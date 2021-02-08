using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using IMDG.Common;
using IMDG.Parser;
using Newtonsoft.Json;

namespace IMDG.Manager
{
    public class Storage
    {
        public Guid Id { get; set; }
        public TcpClient Client { get; set; }

        public TaskCompletionSource<bool> TaskCompletionSource { get; set; }
        public PartitionRange Range { get; set; }
    }

    public class Manager
    {
        private readonly ChannelWriter<Command> _tx;

        private readonly ConcurrentDictionary<Guid, Storage> _clients =
            new ConcurrentDictionary<Guid, Storage>();

        public Manager(ChannelWriter<Command> tx)
        {
            _tx = tx;
        }


        private static IList<PartitionRange> NewRanges(int count)
        {
            if (count < 0) throw new ArgumentException("", nameof(count));
            if(count == 0) return new List<PartitionRange>();
            const long max = (long) int.MaxValue;
            const long min = (long) int.MinValue;
            var n = (Math.Abs(min) + max);
            var ranges = new List<PartitionRange>(count);
            for (int i = 0; i < count - 1; i++)
            {
                var lower = i * n / count;
                var upper = (i + 1) * n / count;
                
                ranges.Add(new PartitionRange((int)(lower - min), (int)(upper - min - 1)));
            }
            ranges.Add(new PartitionRange((int) ((count - 1) * n / count - min), int.MaxValue));
            return ranges;
        }

        private SemaphoreSlim _rebalance = new SemaphoreSlim(1);

        public async Task HandleStorage(TcpClient client, Guid id)
        {
            await DoRebalance(client, id);
           await _clients[id].TaskCompletionSource.Task;
        }

        private async Task DoRebalance(TcpClient client, Guid id)
        {
            await _rebalance.WaitAsync();
            var newRanges = NewRanges(_clients.Count + 1);
            foreach (var (storage, newRange) in _clients.Values.Zip(newRanges))
            {
                storage.Range = newRange;
            }

            var tcs = new TaskCompletionSource<bool>();
            _clients[id] = new Storage
            {
                Id = id,
                Client = client,
                TaskCompletionSource = tcs,
                Range = newRanges[^1]
            };
            var commands = (await FileHelper.ReadCommandsFromFile(FileWriter.FileName)).ToList();
            
            _rebalance.Release();
            foreach (var c in _clients.Values)
            {
                await SendRequest(new SyncCommand
                {
                    Commands = commands
                }, c);
                await SendRequest(new ReplayCommand(c.Range), c);
            }

         
        }
        
        private async Task DoRebalance()
        {
            await _rebalance.WaitAsync();
            var newRanges = NewRanges(_clients.Count);
            foreach (var (storage, newRange) in _clients.Values.Zip(newRanges))
            {
                storage.Range = newRange;
            }
            var commands = (await FileHelper.ReadCommandsFromFile(FileWriter.FileName)).ToList();
            _rebalance.Release();
            foreach (var c in _clients.Values)
            {
                await SendRequest(new SyncCommand
                {
                    Commands = commands
                }, c);
                await SendRequest(new ReplayCommand(c.Range), c);
            }
        }

        public async Task<string> HandleRequest(string req)
        {
            await _rebalance.WaitAsync();
            _rebalance.Release();
            if (_clients.IsEmpty)
            {
                await Console.Error.WriteLineAsync("No storage");
                // TODO: Handle no clients
                return "No storages connected";
            }

           
            var parser = new Parser.Parser(req);
            var command = parser.Parse();
            if (parser.HasErrors)
            {
                var join = string.Join('\n', parser.Errors);
                return join;
            }

            await _tx.WriteAsync(command);
            if (command is KeyCommand kc)
            {
                var keyHash = kc.Key.GetDeterministicHashCode();
                var storage = _clients.Values.First(v => v.Range.Contains(keyHash));
                return await SendRequest(command, storage);
            }

            var sb = new StringBuilder();
            
            foreach (var (_, client) in _clients)
            {
                sb.AppendJoin('\n', await SendRequest(command, client));
            }
            
            return sb.ToString();
        }

        private async Task<string> SendRequest(Command req, Storage client)
        {
            try
            {
                // NOTE: don't dispose
                var networkStream = client.Client.GetStream();
                using var writeMemoryOwner = MemoryPool<byte>.Shared.Rent(100 * 1024);
                var writeMemory = writeMemoryOwner.Memory;
                using var readMemoryOwner = MemoryPool<byte>.Shared.Rent(100 * 1024);
                var readMemory = readMemoryOwner.Memory;


                var ser = JsonConvert.SerializeObject(req, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
                var bytes = Encoding.UTF8.GetBytes(ser, writeMemory.Span);
                await networkStream.WriteAsync(writeMemory[..bytes]);
                await networkStream.FlushAsync();
                var sb = new StringBuilder();
                int length = await networkStream.ReadAsync(readMemory);
                var answer = Encoding.UTF8.GetString(readMemory.Span[..length]);
                sb.Append(answer);
                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType());
                Console.WriteLine($"Storage {client.Id:N} not available");
                client.TaskCompletionSource.SetResult(true);
                _clients.Remove(client.Id, out _);
                await DoRebalance();
                return "Error: Client not available";
            }
        }

        public async Task SendSync(IEnumerable<Command> commands)
        { 
            var sync = new SyncCommand
            {
                Commands = commands.ToList()
            };
            await _rebalance.WaitAsync();
            _rebalance.Release();
            foreach (var storage in _clients.Values)
            {
                await SendRequest(sync, storage);
            }
        }
    }
}