using Server.Models;
using Server.Network;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server.Core
{
    internal class ServerManager
    {
        public event EventHandler<(string clientId, string computerName)>? ClientConnected;
        public event EventHandler<string>? ClientDisconnected;

        private readonly TcpServerListener _tcpListener;
        private readonly ConcurrentDictionary<string, ClientSession> _clients = new ConcurrentDictionary<string, ClientSession>();

        public ServerManager(int port)
        {
            _tcpListener = new TcpServerListener(port);
            _tcpListener.ClientConnected += OnClientConnected;
        }

        public void Start()
        {
            _tcpListener.Start();
            Console.WriteLine("[SERVER-MANAGER] ✓ Server started, listening for connections");
        }

        public void Stop()
        {
            Console.WriteLine("[SERVER-MANAGER] Stopping server...");
            _tcpListener.Stop();
            foreach (var client in _clients.Values) client.Dispose();
            _clients.Clear();
            Console.WriteLine("[SERVER-MANAGER] ✓ Server stopped");
        }

        private async void OnClientConnected(object? sender, TcpClient tcpClient)
        {
            var session = new ClientSession(tcpClient);
            try
            {
                await session.InitializeAsync();
                if (_clients.TryAdd(session.ClientId, session))
                {
                    Console.WriteLine($"[SERVER-MANAGER] ✓ New client: {session.ClientId} ({session.ComputerName})");
                    ClientConnected?.Invoke(this, (session.ClientId, session.ComputerName));
                    _ = MonitorClient(session);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER-MANAGER] ✗ Failed to initialize client: {ex.Message}");
                session.Dispose();
            }
        }

        public async Task SendCommandToClientAsync(string clientId, ActionType type)
        {
            if (_clients.TryGetValue(clientId, out var session))
            {
                Console.WriteLine($"[SERVER-MANAGER] → Sending {type} to {clientId}");
                var action = new RemoteAction { Type = type };
                await session.SendActionAsync(action);
                Console.WriteLine($"[SERVER-MANAGER] ✓ Command sent");
            }
            else
            {
                Console.WriteLine($"[SERVER-MANAGER] ✗ Client {clientId} not found");
            }
        }

        private Task MonitorClient(ClientSession session)
        {
            return Task.Run(async () =>
            {
                while (session.IsConnected())
                {
                    await Task.Delay(2000);
                }

                Console.WriteLine($"[SERVER-MANAGER] Client {session.ClientId} connection lost");
                if (_clients.TryRemove(session.ClientId, out var removedSession))
                {
                    removedSession.Dispose();
                    ClientDisconnected?.Invoke(this, session.ClientId);
                }
            });
        }
    }
}