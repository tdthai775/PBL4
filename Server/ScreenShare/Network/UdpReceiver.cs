using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server.ScreenShare.Network
{
    internal class UdpReceiver : IDisposable
    {
        private readonly UdpClient _udpClient;
        private CancellationTokenSource? _cts;
        private Task? _listenTask;

        public event Action<byte[]>? PacketReceived;

        public UdpReceiver(int port)
        {
            _udpClient = new UdpClient(port);
            _udpClient.Client.ReceiveBufferSize = 1024 * 1024; 
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listenTask = Task.Run(ListenLoop, _cts.Token);
            Console.WriteLine($"SERVER: Bat dau lang nghe tren cong {_udpClient.Client.LocalEndPoint}...");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listenTask?.Wait(500);
            Console.WriteLine("SERVER: Da dung lang nghe.");
        }

        private void ListenLoop()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (_cts != null && !_cts.IsCancellationRequested)
            {
                try
                {
                    byte[] receivedBytes = _udpClient.Receive(ref remoteEndPoint);
                    PacketReceived?.Invoke(receivedBytes);
                }
                catch (SocketException) { if (_cts.IsCancellationRequested) break; }
                catch (Exception ex) { Console.WriteLine($"Loi UdpReceiver: {ex.Message}"); }
            }
        }

        public void Dispose()
        {
            Stop();
            _udpClient?.Close();
            _cts?.Dispose();
        }
    }
}