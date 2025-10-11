using System;
using System.Net;
using System.Net.Sockets;

namespace Client.ScreenShare.Network
{
    /// <summary>
    /// Gửi dữ liệu qua UDP
    /// </summary>
    internal class UdpSender : IDisposable
    {
        private readonly UdpClient _client;
        private readonly IPEndPoint _remoteEndPoint;
        private long _totalBytesSent = 0;
        private int _totalPacketsSent = 0;

        public long TotalBytesSent => _totalBytesSent;
        public int TotalPacketsSent => _totalPacketsSent;

        public UdpSender(string remoteIp, int remotePort)
        {
            _client = new UdpClient();
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);

            // Tăng buffer size cho UDP
            _client.Client.SendBufferSize = 65536;
        }

        /// <summary>
        /// Gửi 1 packet
        /// </summary>
        public void Send(byte[] data)
        {
            try
            {
                int bytesSent = _client.Send(data, data.Length, _remoteEndPoint);
                _totalBytesSent += bytesSent;
                _totalPacketsSent++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP Send Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gửi nhiều packets (chunks)
        /// </summary>
        public void SendChunks(System.Collections.Generic.List<byte[]> chunks)
        {
            foreach (var chunk in chunks)
            {
                Send(chunk);
            }
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStats()
        {
            _totalBytesSent = 0;
            _totalPacketsSent = 0;
        }

        /// <summary>
        /// Lấy thông tin bandwidth (KB/s)
        /// </summary>
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