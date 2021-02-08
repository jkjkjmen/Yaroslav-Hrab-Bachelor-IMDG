using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IMDG.Common;
using Newtonsoft.Json;

namespace IMDG.Storage
{
    public static class StorageMain
    {
        private static async Task Main(string[] args)
        {
            var fh = new FileHandler();
            var storage = new Storage(fh);
            const int port = 7001;
            var pool = MemoryPool<byte>.Shared;
            using var byteRead = pool.Rent(100 * 1024);
            using var byteWrite = pool.Rent(100 * 1024);
            try
            {
                using var client = new TcpClient("127.0.0.1", port);
                await using var stream = client.GetStream();
                while (true)
                {
                    var request = await GetRequest(stream, byteRead.Memory);
                    if (request == null) continue;
                    Console.WriteLine($"[Storage] got {request.Kind}");
                    var answer =  await CheckAndPerformRequest(request, storage);
                    Console.WriteLine($"[Storage] Send back {answer}");
                    await SendAnswer(stream, answer, byteWrite.Memory);
                }

                //client.Close();
                //Console.WriteLine("Client closed the connection");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }
        private static async Task<Command> GetRequest(NetworkStream stream, Memory<byte> byteRead)
        {
            var length = await stream.ReadAsync(byteRead);
            var request = Encoding.UTF8.GetString(byteRead.Span.Slice(0, length));
            Console.WriteLine(request);
            return JsonConvert.DeserializeObject<Command>(request.Trim(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
        }

        private static async Task SendAnswer(Stream stream, string answer, Memory<byte> byteWrite)
        {
            var len = Encoding.UTF8.GetBytes(answer, byteWrite.Span);
            await stream.WriteAsync(byteWrite.Slice(0, len));
            await stream.FlushAsync();
        }
        private static async Task<string> CheckAndPerformRequest(Command request, Storage storage)
        {
            if (request is null)
            {
                // never happens
                return "Internal error";
            }

            return await storage.RequestAsync(request);
        }
    }
}