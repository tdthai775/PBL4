using Server.ScreenShare.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Network
{
    internal class UdpStreamReceiver : IDisposable
    {
        private readonly UdpClient _udpListener;
        private readonly ScreenReceiverPipeline _pipeline;
        private CancellationTokenSource? _cts;
        private int _packetCount = 0;

        public UdpStreamReceiver(int port, ScreenReceiverPipeline pipeline)
        {
            _udpListener = new UdpClient(port);
            _pipeline = pipeline;
            Console.WriteLine($"[SERVER-NET] UDP listener created on port {port}");
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(ListenLoop, _cts.Token);
            Console.WriteLine("[SERVER-NET] ✓ UDP listener started");
        }

        public void Stop()
        {
            Console.WriteLine("[SERVER-NET] Stopping UDP listener...");
            _cts?.Cancel();
            _udpListener.Close();
            Console.WriteLine($"[SERVER-NET] ✓ Stopped (received {_packetCount} packets total)");
        }

        private void ListenLoop()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (_cts != null && !_cts.IsCancellationRequested)
            {
                try
                {
                    byte[] data = _udpListener.Receive(ref remoteEndPoint);
                    _packetCount++;

                    // Log mỗi 100 packets để tránh spam
                    if (_packetCount % 100 == 1)
                    {
                        Console.WriteLine($"[SERVER-NET] Receiving packets from {remoteEndPoint} (packet #{_packetCount})");
                    }

                    _pipeline.ProcessPacket(data, remoteEndPoint.ToString());
                }
                catch (SocketException)
                {
                    if (_cts?.IsCancellationRequested == true) break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER-NET] Error: {ex.Message}");
                }
            }
        }

        public void Dispose() => Stop();
    }
}