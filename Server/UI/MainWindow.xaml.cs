using Server.Core;  
using Server.Models; 
using Server.Network;
using Server.ScreenShare.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                _serverManager.Stop();
                _udpStreamReceiver?.Stop();
                _serverManager = null;
                _udpStreamReceiver = null;
                _screenReceiverPipeline = null;
                _connectedClients.Clear();
                UpdateServerUI(false);
            }
            else
            {
                try
                {
                    int tcpPort = int.Parse(PortTextBox.Text);
                    const int udpPort = 9999;
                    _serverManager = new ServerManager(tcpPort);
                    _serverManager.ClientConnected += OnClientConnected;
                    _serverManager.ClientDisconnected += OnClientDisconnected;
                    _screenReceiverPipeline = new ScreenReceiverPipeline();
                    _udpStreamReceiver = new UdpStreamReceiver(udpPort, _screenReceiverPipeline);
                    _serverManager.Start();
                    _udpStreamReceiver.Start();
                    UpdateServerUI(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khởi động server: {ex.Message}", "Lỗi");
                }
            }
        }

        private async void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientViewModel client)
            {
                if (MessageBox.Show($"Tắt máy client '{client.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    await _serverManager!.SendCommandToClientAsync(client.ClientId, ActionType.Shutdown);
            }
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientViewModel client)
            {
                if (MessageBox.Show($"Khởi động lại client '{client.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    await _serverManager!.SendCommandToClientAsync(client.ClientId, ActionType.Restart);
            }
        }

        private async void StreamScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientViewModel client && _screenReceiverPipeline != null)
            {
                await _serverManager!.SendCommandToClientAsync(client.ClientId, ActionType.StartStream);
                await Task.Delay(500);
                var streamWindow = new ScreenShare(_screenReceiverPipeline, client.ClientId);
                streamWindow.Closed += async (s, args) =>
                {
                    await _serverManager.SendCommandToClientAsync(client.ClientId, ActionType.StopStream);
                };
                streamWindow.ShowDialog();
            }
        }

        private void NotImplemented_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Chức năng này sẽ được phát triển sau.", "Thông báo");

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
                _connectedClients.Add(new ClientViewModel(clientInfo.clientId, clientInfo.computerName));
                ClientCountText.Text = _connectedClients.Count.ToString();
            });
        }

        private void OnClientDisconnected(object? sender, string clientId)
        {
            Dispatcher.Invoke(() =>
            {
                var clientToRemove = _connectedClients.FirstOrDefault(c => c.ClientId == clientId);
                if (clientToRemove != null) _connectedClients.Remove(clientToRemove);
                ClientCountText.Text = _connectedClients.Count.ToString();
            });
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _serverManager?.Stop();
            _udpStreamReceiver?.Stop();
        }
    }
}