using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Client.ScreenShare.Network
{
    internal class UdpSender : IDisposable
    {
        private readonly UdpClient _client;
        private readonly IPEndPoint _remoteEndPoint;
        private long _totalBytesSent = 0;

        public UdpSender(string remoteIp, int remotePort)
        {
            _client = new UdpClient();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
            _client.Client.SendBufferSize = 1024 * 1024; // 1 MB
        }

        public void SendChunks(List<byte[]> chunks)
        {
            foreach (var chunk in chunks)
            {
                try
                {
                    int bytesSent = _client.Send(chunk, chunk.Length, _remoteEndPoint);
                    _totalBytesSent += bytesSent;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UDP Send Error: {ex.Message}");
                }
            }
        }

        public double GetBandwidthKBps(double elapsedSeconds)
        {
            if (elapsedSeconds <= 0) return 0;
            return (_totalBytesSent / 1024.0) / elapsedSeconds;
        }

        public void Dispose()
        {
            _client?.Close();
        }
    }
}