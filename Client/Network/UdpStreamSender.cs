using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    internal class UdpStreamSender : IDisposable
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _remoteEndPoint;
        private readonly PacketChunker _chunker;

        public UdpStreamSender(string remoteIp, int remotePort)
        {
            _udpClient = new UdpClient();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
            _chunker = new PacketChunker();
        }

        public async Task SendFrameAsync(byte[] frameData)
        {
            var chunks = _chunker.CreateFullFrameChunks(frameData);
            foreach (var chunk in chunks)
            {
                await _udpClient.SendAsync(chunk, chunk.Length, _remoteEndPoint);
            }
        }

        public void Dispose() => _udpClient?.Close();
    }
}
