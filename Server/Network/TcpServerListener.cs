using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Network
{
    internal class TcpServerListener
    {
        public event EventHandler<TcpClient>? ClientConnected;
        private readonly TcpListener _listener;
        private CancellationTokenSource? _cts;
        public TcpListener GetListener() => _listener;


        public TcpServerListener(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            Task.Run(ListenLoop, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener.Stop();
        }

        private async Task ListenLoop()
        {
            while (_cts != null && !_cts.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);
                    ClientConnected?.Invoke(this, tcpClient);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception) { /* Xử lý lỗi */ }
            }
        }
    }
}
