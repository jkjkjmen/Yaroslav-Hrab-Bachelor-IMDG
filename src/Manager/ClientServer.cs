using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IMDG.Manager
{
    public class ClientServer : TcpServerBase
    {
        private readonly Manager _manager;


        public ClientServer(Manager manager, int port = 7000) : base(port)
        {
            _manager = manager;
        }

        protected override string Name => "Client Server";

        protected override async Task HandleConnectionAsync(TcpClient tcpClient, Guid _)
        {
            await using var stream = tcpClient.GetStream();
            var pool = MemoryPool<byte>.Shared;
            using var byteRead = pool.Rent(100 * 1024);
            using var byteWrite = pool.Rent(100 * 1024);
            while (tcpClient.Connected)
            {
                var request = await GetRequest(stream, byteRead.Memory);
                var handleRequest = await _manager.HandleRequest(request) + '\n';
                await SendAnswer(stream, handleRequest, byteWrite.Memory);
            }
        }

        private static async Task<string> GetRequest(Stream stream, Memory<byte> byteRead)
        {
            var length = await stream.ReadAsync(byteRead);
            var request = Encoding.UTF8.GetString(byteRead.Span.Slice(0, length));
            return request;
        }

        private static async Task SendAnswer(Stream stream, string answer, Memory<byte> byteWrite)
        {
            int len = Encoding.UTF8.GetBytes(answer, byteWrite.Span);
            await stream.WriteAsync(byteWrite.Slice(0, len));
            await stream.FlushAsync();
        }
    }
}