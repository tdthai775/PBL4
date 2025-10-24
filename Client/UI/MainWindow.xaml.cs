using Client.Models;
using Client.Network;
using Client.ScreenShare;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Client.UI
{
    public partial class MainWindow : Window
    {
        private TcpCommandChannel? _commandChannel;
        private UdpStreamSender? _udpSender;
        private ScreenSharePipeline? _screenSharePipeline;
        private CancellationTokenSource? _streamingCts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ConnectDisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_commandChannel != null && _commandChannel.IsConnected)
            {
                StopServices();
                UpdateUI(false);
            }
            else
            {
                string serverIp = ServerIpTextBox.Text;
                if (!int.TryParse(PortTextBox.Text, out int tcpPort))
                {
                    MessageBox.Show("Port phải là một con số hợp lệ.", "Lỗi");
                    return;
                }
                const int udpPort = 9999;

                try
                {
                    _commandChannel = new TcpCommandChannel(serverIp, tcpPort);
                    _commandChannel.ActionReceived += OnActionReceived;
                    await _commandChannel.ConnectAsync();
                    _udpSender = new UdpStreamSender(serverIp, udpPort);
                    UpdateUI(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể kết nối: {ex.Message}", "Lỗi");
                    StopServices();
                }
            }
        }

        private void OnActionReceived(object? sender, RemoteAction action)
        {
            Dispatcher.Invoke(() =>
            {
                if (action.Type == ActionType.StartStream) StartStreaming();
                else if (action.Type == ActionType.StopStream) StopStreaming();
            });
        }

        private void StartStreaming()
        {
            if (_screenSharePipeline != null || _udpSender == null) return;

            _screenSharePipeline = new ScreenSharePipeline(_udpSender);
            _streamingCts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _screenSharePipeline.StartAsync(_streamingCts.Token);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Lỗi streaming: {ex.Message}");
                        StopStreaming();
                    });
                }
            }, _streamingCts.Token);
        }

        private void StopStreaming()
        {
            _streamingCts?.Cancel();
            _screenSharePipeline?.Dispose();
            _streamingCts = null;
            _screenSharePipeline = null;
        }

        private void StopServices()
        {
            StopStreaming();
            _commandChannel?.Disconnect();
            _udpSender?.Dispose();

            _commandChannel = null;
            _udpSender = null;
        }

        private void UpdateUI(bool isConnected)
        {
            if (isConnected)
            {
                StatusText.Text = "Đã kết nối";
                StatusEllipse.Fill = (SolidColorBrush)FindResource("BrushAccentGreen");
                StatusText.Foreground = (SolidColorBrush)FindResource("BrushAccentGreen");
                ConnectText.Text = "Ngắt kết nối";
                ConnectIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.PowerPlugOff;
                ConnectDisconnectButton.Background = (SolidColorBrush)FindResource("BrushAccentRed");
                ServerIpTextBox.IsEnabled = false;
                PortTextBox.IsEnabled = false;
            }
            else
            {
                StatusText.Text = "Đã ngắt kết nối";
                StatusEllipse.Fill = (SolidColorBrush)FindResource("BrushAccentRed");
                StatusText.Foreground = (SolidColorBrush)FindResource("BrushAccentRed");
                ConnectText.Text = "Kết nối";
                ConnectIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.LanConnect;
                ConnectDisconnectButton.Background = (SolidColorBrush)FindResource("BrushAccentBlue");
                ServerIpTextBox.IsEnabled = true;
                PortTextBox.IsEnabled = true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            StopServices();
            base.OnClosing(e);
        }
    }
}