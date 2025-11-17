using Server.ScreenShare.Core;
using Server.ScreenShare.Renderer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;

namespace Server.UI
{
    public partial class ScreenShare : Window
    {
        private readonly ScreenReceiverPipeline _pipeline;
        private readonly string _clientId;
        private readonly ScreenRenderer _renderer;

        private int _frameCount = 0; 
        private int _fpsFrameCount = 0;
        private readonly Stopwatch _fpsStopwatch = new Stopwatch();
        private readonly DispatcherTimer _fpsTimer;

        public ScreenShare(ScreenReceiverPipeline pipeline, string clientId)
        {
            InitializeComponent();
            _pipeline = pipeline;
            _clientId = clientId;
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
                //FpsText.Text = fps.ToString("F1");
                _fpsFrameCount = 0;
                _fpsStopwatch.Restart();
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

            _pipeline.FrameReady -= OnFrameReady;
            _pipeline.SetTargetClient(null);

            Console.WriteLine($"[SCREENVIEW] ✓ Cleanup done. Total frames: {_frameCount}");
        }
    }
}