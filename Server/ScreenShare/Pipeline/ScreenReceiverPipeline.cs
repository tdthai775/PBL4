using System;
using System.Windows.Controls;
using Server.ScreenShare.Decoder;
using Server.ScreenShare.Network;
using Server.ScreenShare.Renderer;

namespace Server.ScreenShare.Pipeline
{
    internal class ScreenReceiverPipeline : IDisposable
    {
        private readonly UdpReceiver _receiver;
        private readonly PacketAssembler _assembler;
        private readonly JpegDecoder _decoder;
        private readonly FrameManager _frameManager;
        private readonly ScreenRenderer _renderer;

        public ScreenReceiverPipeline(int port, Image imageControl)
        {
            _receiver = new UdpReceiver(port);
            _assembler = new PacketAssembler();
            _decoder = new JpegDecoder();
            _frameManager = new FrameManager();
            _renderer = new ScreenRenderer(imageControl);

            _receiver.PacketReceived += _assembler.ProcessPacket;
            _assembler.FrameAssembled += OnFrameAssembled;
        }

        private void OnFrameAssembled(AssembledFrame frame)
        {
            using (var bmp = _decoder.Decode(frame.JpegData))
            {
                if (bmp == null) return;

                _frameManager.UpdateFrame(bmp, frame.PatchLocation);

                // Lấy bức tranh hoàn chỉnh cuối cùng
                using (var finalFrame = _frameManager.GetCurrentFrame())
                {
                    if (finalFrame != null)
                        _renderer.Render(finalFrame);
                }
            }
        }

        public void Start() => _receiver.Start();
        public void Stop() => _receiver.Stop();

        public void Dispose()
        {
            Stop();
            _receiver.Dispose();
            _frameManager.Dispose();
        }
    }
}