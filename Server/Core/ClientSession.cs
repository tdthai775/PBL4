using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Core
{
    internal class ClientSession : IDisposable
    {
        public string ClientId { get; }
        public string ComputerName { get; private set; } = "Unknown";
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        public TcpClient GetTcpClient() => _tcpClient;

        public ClientSession(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _stream = tcpClient.GetStream();
            ClientId = tcpClient.Client.RemoteEndPoint!.ToString()!;
        }

        public async Task InitializeAsync()
        {
            using var reader = new StreamReader(_stream, leaveOpen: true);
            ComputerName = await reader.ReadLineAsync() ?? "Unknown";
        }

        public async Task SendActionAsync(RemoteAction action)
        {
            if (!_tcpClient.Connected) return;
            try
            {
                var jsonAction = JsonSerializer.Serialize(action);
                var writer = new StreamWriter(_stream);
                await writer.WriteLineAsync(jsonAction);
                await writer.FlushAsync();
            }
            catch (IOException) { Disconnect(); }
        }

        public bool IsConnected()
        {
            try
            {
                return !(_tcpClient.Client.Poll(1, SelectMode.SelectRead) && _tcpClient.Client.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public void Disconnect() => _tcpClient?.Close();
        public void Dispose() => Disconnect();
    }
}
