using Server.Core;
using Server.Models;
using Server.Network;
using Server.ScreenShare.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace Server.UI
{
    public partial class MainWindow : Window
    {
        private ServerManager? _serverManager;
        private ScreenReceiverPipeline? _screenReceiverPipeline;
        private UdpStreamReceiver? _udpStreamReceiver;
        private readonly ObservableCollection<ClientViewModel> _connectedClients = new ObservableCollection<ClientViewModel>();

        public MainWindow()
        {
            InitializeComponent();
            ClientListBox.ItemsSource = _connectedClients;
            Closing += OnWindowClosing;
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_serverManager != null)
            {
                Console.WriteLine("[SERVER] Stopping server...");
                _serverManager.Stop();
                _udpStreamReceiver?.Stop();
                _serverManager = null;
                _udpStreamReceiver = null;
                _screenReceiverPipeline = null;
                _connectedClients.Clear();
                UpdateServerUI(false);
                Console.WriteLine("[SERVER] ✓ Server stopped");
            }
            else
            {
                try
                {
                    int tcpPort = int.Parse(PortTextBox.Text);
                    const int udpPort = 9999;

                    Console.WriteLine($"[SERVER] Starting on TCP:{tcpPort}, UDP:{udpPort}");

                    _serverManager = new ServerManager(tcpPort);
                    _serverManager.ClientConnected += OnClientConnected;
                    _serverManager.ClientDisconnected += OnClientDisconnected;

                    _screenReceiverPipeline = new ScreenReceiverPipeline();
                    _udpStreamReceiver = new UdpStreamReceiver(udpPort, _screenReceiverPipeline);

                    _serverManager.Start();
                    _udpStreamReceiver.Start();

                    UpdateServerUI(true);
                    Console.WriteLine("[SERVER] ✓ Server started successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER] ✗ Failed to start: {ex.Message}");
                    MessageBox.Show($"Lỗi khởi động server: {ex.Message}", "Lỗi");
                }
            }
        }

        private async void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientViewModel client)
            {
                if (MessageBox.Show($"Tắt máy client '{client.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Console.WriteLine($"[SERVER] Sending Shutdown to {client.ClientId}");
                    await _serverManager!.SendCommandToClientAsync(client.ClientId, ActionType.Shutdown);
                }
            }
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientViewModel client)
            {
                if (MessageBox.Show($"Khởi động lại client '{client.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Console.WriteLine($"[SERVER] Sending Restart to {client.ClientId}");
                    await _serverManager!.SendCommandToClientAsync(client.ClientId, ActionType.Restart);
                }
            }
        }

        private async void StreamScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientViewModel client && _screenReceiverPipeline != null)
            {
                if (_serverManager == null)
                {
                    MessageBox.Show("Server chưa được khởi động.", "Lỗi");
                    return;
                }

                try
                {
                    Console.WriteLine($"[SERVER] → Sending StartStream to {client.ClientId}");
                    await _serverManager.SendCommandToClientAsync(client.ClientId, ActionType.StartStream);

                    Console.WriteLine($"[SERVER] Waiting 500ms for client to start...");
                    await Task.Delay(500);

                    Console.WriteLine($"[SERVER] Opening stream window...");
                    var streamWindow = new ScreenShare(_screenReceiverPipeline, client.ClientId, _serverManager);

                    streamWindow.Closed += async (s, args) =>
                    {
                        Console.WriteLine($"[SERVER] → Sending StopStream to {client.ClientId}");
                        if (_serverManager != null)
                        {
                            await _serverManager.SendCommandToClientAsync(client.ClientId, ActionType.StopStream);
                        }
                    };

                    streamWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER] ✗ Error: {ex.Message}");
                    MessageBox.Show($"Lỗi khi bắt đầu stream: {ex.Message}", "Lỗi");
                }
            }
        }

        private async void TaskManagerButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientViewModel client)
            {
                if (_serverManager == null)
                {
                    MessageBox.Show("Server chưa được khởi động.", "Lỗi");
                    return;
                }
                try
                {
                    Console.WriteLine($"[SERVER] → Sending RequestProcessList to {client.ClientId}");
                    await _serverManager.SendCommandToClientAsync(client.ClientId, ActionType.RequestProcessList);
                    var found = _serverManager.GetClients().Values.FirstOrDefault(c => c.ClientId == client.ClientId);
                    var taskManagerWindow = new TaskManagerUI(_serverManager.GetTcpServerListener().GetListener(), found.GetTcpClient());
                    taskManagerWindow.Show();

                    taskManagerWindow.Closed += async (s, args) =>
                    {
                        Console.WriteLine($"[SERVER] Task Manager window closed for {client.ClientId}");
                        await _serverManager.SendCommandToClientAsync(client.ClientId, ActionType.StopSendingProcessList);
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER] ✗ Error: {ex.Message}");
                    MessageBox.Show($"Lỗi khi yêu cầu danh sách tiến trình: {ex.Message}", "Lỗi");
                }
            }
        }

        private void UpdateServerUI(bool isRunning)
        {
            if (isRunning)
            {
                ServerStatusText.Text = "Đang chạy";
                ServerStatusEllipse.Fill = (SolidColorBrush)FindResource("BrushAccentGreen");
                ServerStatusText.Foreground = (SolidColorBrush)FindResource("BrushAccentGreen");
                StartStopText.Text = "Dừng Server";
                StartStopIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop;
                StartStopButton.Background = (SolidColorBrush)FindResource("BrushAccentRed");
                PortTextBox.IsEnabled = false;
            }
            else
            {
                ServerStatusText.Text = "Đã dừng";
                ServerStatusEllipse.Fill = (SolidColorBrush)FindResource("BrushAccentRed");
                ServerStatusText.Foreground = (SolidColorBrush)FindResource("BrushAccentRed");
                StartStopText.Text = "Khởi Động Server";
                StartStopIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                StartStopButton.Background = (SolidColorBrush)FindResource("BrushAccentGreen");
                PortTextBox.IsEnabled = true;
                ClientCountText.Text = "0";
            }
        }

        private void OnClientConnected(object? sender, (string clientId, string computerName) clientInfo)
        {
            Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"[SERVER] ✓ Client connected: {clientInfo.clientId} ({clientInfo.computerName})");
                _connectedClients.Add(new ClientViewModel(clientInfo.clientId, clientInfo.computerName));
                ClientCountText.Text = _connectedClients.Count.ToString();
            });
        }

        private void OnClientDisconnected(object? sender, string clientId)
        {
            Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"[SERVER] ✗ Client disconnected: {clientId}");
                var clientToRemove = _connectedClients.FirstOrDefault(c => c.ClientId == clientId);
                if (clientToRemove != null) _connectedClients.Remove(clientToRemove);
                ClientCountText.Text = _connectedClients.Count.ToString();
            });
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            Console.WriteLine("[SERVER] Main window closing...");
            _serverManager?.Stop();
            _udpStreamReceiver?.Stop();
        }
    }
}