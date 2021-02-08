using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IMDG.Manager
{
    public abstract class TcpServerBase
    {
        private readonly List<Task> _connections = new List<Task>();
        private readonly int _port;
        private bool _isRunning;

        protected TcpServerBase(int port = 7000)
        {
            _port = port;
        }


        protected virtual string Name { get; } = "Server";

        public async Task Start()
        {
            var tcpListener = TcpListener.Create(_port);
            tcpListener.Start();
            _isRunning = true;
            while (_isRunning)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                var id = Guid.NewGuid();
                Console.WriteLine($"[{Name}] Client {id:N} connected");
                // launch new thread
                var task = StartHandleConnectionAsync(tcpClient, id);
                // if already faulted, re-throw any error on the calling context
                if (task.IsFaulted)
                    await task;
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }

        protected abstract Task HandleConnectionAsync(TcpClient tcpClient, Guid clientId);

        protected virtual void OnShutdown(Guid clientId)
        {
        }

        // Register and handle the connection
        private async Task StartHandleConnectionAsync(TcpClient tcpClient, Guid clientId)
        {
            // start the new connection task
            var connectionTask = Task.Run(() => HandleConnectionAsync(tcpClient, clientId));

            // add it to the list of pending task
            lock (_connections)
            {
                _connections.Add(connectionTask);
            }
            // catch all errors of HandleConnectionAsync
            try
            {
                await connectionTask;
                // we may be on another thread after "await"
            }
            catch (Exception ex)
            {
                // log the error
                Console.WriteLine(ex.Message);
                Console.WriteLine($"[Server] Client {clientId} disconnected");
            }
            finally
            {
                lock (_connections)
                {
                    OnShutdown(clientId);
                    _connections.Remove(connectionTask);
                }
                tcpClient.Dispose();
            }
        }
    }
}