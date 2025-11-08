using Server.ScreenShare.Core;
using Server.ScreenShare.Renderer;
using Server.RemoteControl;
using Server.Models;
using Server.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;

// ✅ Using aliases to avoid ambiguity
using Color = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace Server.UI
{
    public partial class ScreenShare : Window
    {
        private readonly ScreenReceiverPipeline _pipeline;
        private readonly string _clientId;
        private readonly ScreenRenderer _renderer;
        private readonly ServerManager _serverManager;
        private InputCapture? _inputCapture;
        private bool _isControlEnabled = false;

        private int _frameCount = 0;
        private int _fpsFrameCount = 0;
        private readonly Stopwatch _fpsStopwatch = new Stopwatch();
        private readonly DispatcherTimer _fpsTimer;

        public ScreenShare(ScreenReceiverPipeline pipeline, string clientId, ServerManager serverManager)
        {
            InitializeComponent();
            _pipeline = pipeline;
            _clientId = clientId;
            _serverManager = serverManager;
            _renderer = new ScreenRenderer(StreamImage);
            this.Title = $"Streaming from: {clientId}";

            _fpsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _fpsTimer.Tick += UpdateFpsDisplay;

            Console.WriteLine($"[SCREENVIEW] Window created for client: {clientId}");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"[SCREENVIEW] Window loaded, registering event handler...");

            _pipeline.FrameReady += OnFrameReady;
            _pipeline.SetTargetClient(_clientId);

            _fpsStopwatch.Start();
            _fpsTimer.Start();

            // Initialize InputCapture (default: disabled)
            _inputCapture = new InputCapture(StreamImage, 1920, 1080);
            _inputCapture.InputActionReceived += OnInputActionReceived;

            Console.WriteLine($"[SCREENVIEW] ✓ Ready, waiting for frames from {_clientId}");
        }

        private void OnFrameReady(object? sender, (string clientId, Bitmap frame) data)
        {
            string dataIp = GetIpOnly(data.clientId);
            string targetIp = GetIpOnly(_clientId);

            if (dataIp == targetIp)
            {
                _frameCount++;
                _fpsFrameCount++;

                if (_frameCount % 30 == 1)
                {
                    Console.WriteLine($"[SCREENVIEW] ✓ Frame #{_frameCount} rendered");
                }

                _renderer.Render(data.frame);
            }
            else
            {
                Console.WriteLine($"[SCREENVIEW] Frame from {dataIp} ignored (expecting {targetIp})");
            }

            data.frame.Dispose();
        }

        private void UpdateFpsDisplay(object? sender, EventArgs e)
        {
            double elapsedSeconds = _fpsStopwatch.Elapsed.TotalSeconds;

            if (elapsedSeconds >= 1.0)
            {
                double fps = _fpsFrameCount / elapsedSeconds;
                FpsText.Text = fps.ToString("F1");
                _fpsFrameCount = 0;
                _fpsStopwatch.Restart();
            }
        }

        private void ControlButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isControlEnabled)
            {
                // Disable control
                _inputCapture?.Disable();
                _isControlEnabled = false;

                ControlText.Text = "Bật điều khiển";
                ControlButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC2563EB"));
                ControlIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.GestureTap;
                ControlStatusBorder.Visibility = Visibility.Collapsed;
                StreamImage.Cursor = System.Windows.Input.Cursors.Arrow;

                Console.WriteLine("[SCREENVIEW] Remote control DISABLED");
            }
            else
            {
                // Enable control
                _inputCapture?.Enable();
                _isControlEnabled = true;

                ControlText.Text = "Tắt điều khiển";
                ControlButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCDC2626"));
                ControlIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.GestureTapButton;
                ControlStatusBorder.Visibility = Visibility.Visible;
                StreamImage.Cursor = System.Windows.Input.Cursors.Cross;

                Console.WriteLine("[SCREENVIEW] Remote control ENABLED");
            }
        }

        private async void OnInputActionReceived(object? sender, RemoteAction action)
        {
            try
            {
                // Chỉ log non-MouseMove events để tránh spam
                if (action.Type != ActionType.MouseMove)
                {
                    Console.WriteLine($"[SCREENVIEW] Sending input: {action.Type}");
                }

                await _serverManager.SendCommandToClientAsync(_clientId, action);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCREENVIEW] Error sending input: {ex.Message}");
            }
        }

        private string GetIpOnly(string clientId)
        {
            if (string.IsNullOrEmpty(clientId)) return "";
            int colonIndex = clientId.IndexOf(':');
            return colonIndex > 0 ? clientId.Substring(0, colonIndex) : clientId;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"[SCREENVIEW] Back button clicked, closing...");
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine($"[SCREENVIEW] Window closing, cleanup...");

            _fpsTimer.Stop();
            _fpsStopwatch.Stop();

            _inputCapture?.Disable();
            if (_inputCapture != null)
            {
                _inputCapture.InputActionReceived -= OnInputActionReceived;
            }

            _pipeline.FrameReady -= OnFrameReady;
            _pipeline.SetTargetClient(null);

            Console.WriteLine($"[SCREENVIEW] ✓ Cleanup done. Total frames: {_frameCount}");
        }
    }
}