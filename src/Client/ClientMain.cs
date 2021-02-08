using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IMDG.Client
{
    public static class ClientMain
    {
       static async Task Main(string[] args)
        {
            const int port = 7000;
            var pool = MemoryPool<byte>.Shared;
            using var byteRead = pool.Rent(1024);
            using var byteWrite = pool.Rent(1024);
            try
            {
                using var client = new TcpClient("127.0.0.1", port);
                await using var stream = client.GetStream();
                Console.WriteLine("Connected to the server!");
                while (true)
                {
                    Console.WriteLine("Enter the request:");
                    var request = Console.ReadLine();
                    if (request == "")
                    {
                        Console.WriteLine("No request to send!\n");
                        continue;
                    }
                    await SendRequest(stream, request, byteWrite.Memory);
                    var answer = await GetAnswer(stream, byteRead.Memory);
                }

                //client.Close();
                //Console.WriteLine("Client closed the connection");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        private static async Task<string> GetAnswer(Stream stream, Memory<byte> byteRead)
        {
            var length = await stream.ReadAsync(byteRead);
            var answer = Encoding.UTF8.GetString(byteRead.Span.Slice(0, length));
            Console.WriteLine("Answer: " + "\n" + answer);
            return answer;
        }
        private static async Task SendRequest(Stream stream, string request, Memory<byte> byteWrite)
        {
            int len = Encoding.UTF8.GetBytes(request, byteWrite.Span);
            await stream.WriteAsync(byteWrite.Slice(0, len));
            await stream.FlushAsync();
            Console.WriteLine("Sended: " + request);
        }
     
    }
}