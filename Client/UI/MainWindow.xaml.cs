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
                    Console.WriteLine($"[CLIENT] Connecting to {serverIp}:{tcpPort}...");

                    _commandChannel = new TcpCommandChannel(serverIp, tcpPort);
                    _commandChannel.ActionReceived += OnActionReceived;
                    await _commandChannel.ConnectAsync();

                    _udpSender = new UdpStreamSender(serverIp, udpPort);

                    Console.WriteLine("[CLIENT] Connected successfully!");
                    UpdateUI(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT] Connection failed: {ex.Message}");
                    MessageBox.Show($"Không thể kết nối: {ex.Message}", "Lỗi");
                    StopServices();
                }
            }
        }

        private void OnActionReceived(object? sender, ActionType actionType)
        {
            Console.WriteLine($"[CLIENT-UI] Received action: {actionType}");

            Dispatcher.Invoke(() =>
            {
                if (actionType == ActionType.StartStream)
                {
                    Console.WriteLine("[CLIENT-UI] Starting stream...");
                    StartStreaming();
                }
                else if (actionType == ActionType.StopStream)
                {
                    Console.WriteLine("[CLIENT-UI] Stopping stream...");
                    StopStreaming();
                }
            });
        }

        private void StartStreaming()
        {
            if (_screenSharePipeline != null)
            {
                Console.WriteLine("[CLIENT-UI] Stream already running!");
                return;
            }

            if (_udpSender == null)
            {
                Console.WriteLine("[CLIENT-UI] UDP sender not initialized!");
                return;
            }

            try
            {
                Console.WriteLine("[CLIENT-UI] Initializing screen share pipeline...");
                _screenSharePipeline = new ScreenSharePipeline(_udpSender);
                _streamingCts = new CancellationTokenSource();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine("[CLIENT-PIPELINE] Starting capture loop...");
                        await _screenSharePipeline.StartAsync(_streamingCts.Token);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CLIENT-PIPELINE] Error: {ex.Message}");
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Lỗi streaming: {ex.Message}", "Lỗi");
                            StopStreaming();
                        });
                    }
                }, _streamingCts.Token);

                Console.WriteLine("[CLIENT-UI] Stream started!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT-UI] Failed to start streaming: {ex.Message}");
                MessageBox.Show($"Không thể khởi động streaming: {ex.Message}", "Lỗi");
            }
        }

        private void StopStreaming()
        {
            Console.WriteLine("[CLIENT-UI] Stopping streaming...");

            _streamingCts?.Cancel();
            _screenSharePipeline?.Dispose();
            _streamingCts = null;
            _screenSharePipeline = null;

            Console.WriteLine("[CLIENT-UI] Stream stopped.");
        }

        private void StopServices()
        {
            Console.WriteLine("[CLIENT] Stopping all services...");

            StopStreaming();

            if (_commandChannel != null)
            {
                _commandChannel.ActionReceived -= OnActionReceived;
            }

            _commandChannel?.Disconnect();
            _udpSender?.Dispose();

            _commandChannel = null;
            _udpSender = null;

            Console.WriteLine("[CLIENT] All services stopped.");
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