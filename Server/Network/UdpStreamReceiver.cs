using Server.ScreenShare;
using Server.ScreenShare.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Server.Network
{
    internal class UdpStreamReceiver : IDisposable
    {
        private readonly UdpClient _udpListener;
        private readonly ScreenReceiverPipeline _pipeline;
        private CancellationTokenSource? _cts;

        public UdpStreamReceiver(int port, ScreenReceiverPipeline pipeline)
        {
            _udpListener = new UdpClient(port);
            _pipeline = pipeline;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(ListenLoop, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _udpListener.Close();
        }

        private void ListenLoop()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (_cts != null && !_cts.IsCancellationRequested)
            {
                try
                {
                    byte[] data = _udpListener.Receive(ref remoteEndPoint);
                    _pipeline.ProcessPacket(data, remoteEndPoint.ToString());
                }
                catch (SocketException) { if (_cts?.IsCancellationRequested == true) break; }
            }
        }

        public void Dispose() => Stop();
    }
}
