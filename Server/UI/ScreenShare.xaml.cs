using Server.ScreenShare.Core;
using Server.ScreenShare.Renderer;
using System.ComponentModel;
using System.Drawing;
using System.Windows;

namespace Server.UI
{
    public partial class ScreenShare : Window
    {
        private readonly ScreenReceiverPipeline _pipeline;
        private readonly string _clientId;
        private readonly ScreenRenderer _renderer;

        public ScreenShare(ScreenReceiverPipeline pipeline, string clientId)
        {
            InitializeComponent();
            _pipeline = pipeline;
            _clientId = clientId;
            _renderer = new ScreenRenderer(StreamImage);
            this.Title = $"Streaming from: {clientId}";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _pipeline.FrameReady += OnFrameReady;
            _pipeline.SetTargetClient(_clientId);
        }

        private void OnFrameReady(object? sender, (string clientId, Bitmap frame) data)
        {
            if (data.clientId == _clientId) _renderer.Render(data.frame);
            data.frame.Dispose();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _pipeline.FrameReady -= OnFrameReady;
            _pipeline.SetTargetClient(null);
        }
    }
}