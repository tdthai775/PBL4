using Client.Models;
using Client.Network;
using Client.ScreenShare;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.Reflection;

namespace Client.UI
{
    public partial class MainWindow : Window
    {
        private TcpCommandChannel? _commandChannel;
        private UdpStreamSender? _udpSender;
        private ScreenSharePipeline? _screenSharePipeline;
        private CancellationTokenSource? _streamingCts;
        private AppConfig _config;
        private Thread sendProcessesThread;
        private Thread handleKillCommandThread;

        public MainWindow()
        {
            InitializeComponent();
            _config = AppConfig.Load();
            ServerIpTextBox.Text = _config.ServerIp;
            PortTextBox.Text = _config.ServerPort.ToString();
            sendProcessesThread = new Thread(SendProcessesLoop);
            sendProcessesThread.IsBackground = true;
            

            Loaded += async (s, e) =>
            {
                try
                {
                    await Task.Delay(1000); 
                    await AutoConnectAsync(); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AutoConnect failed: {ex.Message}");
                }
            };
        }
        public static void SetStartup(bool enable)
        {
            string appName = "PBL4Client";
            string exePath = Process.GetCurrentProcess().MainModule?.FileName
                             ?? Assembly.GetExecutingAssembly().Location;

            if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                exePath = Path.ChangeExtension(exePath, ".exe");
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (enable)
                {
                    psi.Arguments = $"/Create /TN \"{appName}\" /TR \"\\\"{exePath}\\\"\" " +
                                  $"/SC ONLOGON /DELAY 0000:10 /F";
                }
                else
                {
                    psi.Arguments = $"/Delete /TN \"{appName}\" /F";
                }

                Process.Start(psi)?.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set startup: {ex.Message}");
            }
        }


        private async Task AutoConnectAsync()
        {
            try
            {
                _commandChannel = new TcpCommandChannel(_config.ServerIp, _config.ServerPort);
                _commandChannel.ActionReceived += OnActionReceived;
                await _commandChannel.ConnectAsync();

                _udpSender = new UdpStreamSender(_config.ServerIp, 9999);
                UpdateUI(true);

                handleKillCommandThread = new Thread(HandleKillCommand);
                handleKillCommandThread.IsBackground = true;
                handleKillCommandThread.Start();

                SetStartup(true);
                Console.WriteLine($"[CLIENT] Auto-connected to {_config.ServerIp}:{_config.ServerPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Auto-connect failed: {ex.Message}");
            }
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

                    //Lưu lại IP và Port mới
                    _config.ServerIp = serverIp;
                    _config.ServerPort = tcpPort;
                    _config.Save();

                    SetStartup(true);

                    Console.WriteLine("[CLIENT] Connected successfully!");
                    UpdateUI(true);
                    handleKillCommandThread = new Thread(HandleKillCommand);
                    handleKillCommandThread.IsBackground = true;
                    handleKillCommandThread.Start();
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
                else if (actionType == ActionType.RequestProcessList)
                {
                    Console.WriteLine("[CLIENT-UI] Received RequestProcessList action - Not implemented in UI.");
                    sendProcessesThread.Start();
                }
            });
        }

        private void HandleKillCommand()
        {
            var stream = _commandChannel.GetTcpClient().GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);

            while (true)
            {
                try
                {
                    string? cmd = reader.ReadLine(); // đọc đến ký tự \n
                    if (string.IsNullOrEmpty(cmd))
                        continue;

                    if (cmd.StartsWith("KillProcess|"))
                    {
                        string[] parts = cmd.Split('|');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int pid))
                        {
                            try
                            {
                                Process.GetProcessById(pid).Kill();
                            }
                            catch { }

                            SendProcess(); // gửi lại danh sách mới
                        }
                        return;
                    }

                    // RequestProcessList
                    if (cmd == "RequestProcessList")
                    {
                        sendProcessesThread.Start();
                        return;
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        private void SendProcess()
        {
            var list = new List<object>();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    list.Add(new
                    {
                        p.Id,
                        p.ProcessName,
                        Memory = $"{p.WorkingSet64 / 1024 / 1024} MB"
                    });
                }
                catch { }
            }

            string json = JsonSerializer.Serialize(list) + "\n"; // <-- delimiter

            byte[] data = Encoding.UTF8.GetBytes(json);
            var stream = _commandChannel.GetTcpClient().GetStream();
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        private void SendProcessesLoop()
        {
            while (true)
            {
                SendProcess();
                Thread.Sleep(1000); 
            }
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