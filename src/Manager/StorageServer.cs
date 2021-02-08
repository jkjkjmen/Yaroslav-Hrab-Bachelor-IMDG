using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IMDG.Manager
{
    public class StorageServer : TcpServerBase
    {
        private readonly Manager _manager;

        public StorageServer(Manager manager, int port = 7001) : base(port)
        {
            _manager = manager;
        }


        protected override string Name => "Storage Server";

        protected override Task HandleConnectionAsync(TcpClient tcpClient, Guid clientId)
        {
            return _manager.HandleStorage(tcpClient, clientId);
        }
    }
}