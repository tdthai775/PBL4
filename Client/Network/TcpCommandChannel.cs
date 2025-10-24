using Client.Core;
using Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Client.Network
{
    internal class TcpCommandChannel : IDisposable
    {
        public event EventHandler<RemoteAction>? ActionReceived;

        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly ActionDispatcher _dispatcher;
        private TcpClient? _tcpClient;
        private CancellationTokenSource? _cts;

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public TcpCommandChannel(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _dispatcher = new ActionDispatcher();
        }

        public async Task ConnectAsync()
        {
            if (IsConnected) return;

            _cts = new CancellationTokenSource();
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_serverIp, _serverPort, _cts.Token);
            var stream = _tcpClient.GetStream();

            var writer = new StreamWriter(stream);
            await writer.WriteLineAsync(Environment.MachineName);
            await writer.FlushAsync();

            _ = ListenForCommandsAsync(stream, _cts.Token);
        }

        private async Task ListenForCommandsAsync(NetworkStream stream, CancellationToken token)
        {
            using var reader = new StreamReader(stream);
            try
            {
                while (!token.IsCancellationRequested && IsConnected)
                {
                    var jsonData = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(jsonData)) break;

                    var action = JsonSerializer.Deserialize<RemoteAction>(jsonData);
                    if (action != null)
                    {
                        ActionReceived?.Invoke(this, action);
                        _dispatcher.Dispatch(action);
                    }
                }
            }
            catch (IOException) { /* Ngắt kết nối */ }
            finally { Disconnect(); }
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            _tcpClient?.Close();
        }

        public void Dispose() => Disconnect();
    }
}
