using Client.Network;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Client.ScreenShare
{
    internal class ScreenSharePipeline : IDisposable
    {
        private readonly UdpStreamSender _udpSender;
        private readonly int _targetWidth, _targetHeight, _jpegQuality, _targetFps;
        private ID3D11Device? _device;
        private ID3D11DeviceContext? _context;
        private IDXGIOutputDuplication? _duplication;
        private int _screenWidth, _screenHeight;
        private readonly ImageCodecInfo _jpegCodec;
        private readonly EncoderParameters _encoderParams;

        public ScreenSharePipeline(UdpStreamSender udpSender, int targetWidth = 1920, int targetHeight = 1080, int jpegQuality = 80, int targetFps = 30)
        {
            _udpSender = udpSender;
            _targetWidth = targetWidth;
            _targetHeight = targetHeight;
            _jpegQuality = jpegQuality;
            _targetFps = targetFps;

            InitializeDirectX();
            _jpegCodec = GetJpegCodec() ?? throw new NotSupportedException("JPEG codec not found.");
            _encoderParams = new EncoderParameters(1);
            _encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)_jpegQuality);
        }
        
        public async Task StartAsync(CancellationToken token)
        {
            var frameDelay = 1000 / _targetFps;
            while (!token.IsCancellationRequested)
            {
                var frameStartTime = Stopwatch.GetTimestamp();
                try
                {
                    using (var frame = CaptureFrame())
                    {
                        if (frame == null) { await Task.Delay(10, token); continue; }
                        using (var resizedFrame = Resize(frame))
                        {
                            byte[] jpegData = Encode(resizedFrame);
                            await _udpSender.SendFrameAsync(jpegData);
                        }
                    }
                }
                catch (Exception) {   }

                var elapsedMs = (Stopwatch.GetTimestamp() - frameStartTime) * 1000.0 / Stopwatch.Frequency;
                int sleep = frameDelay - (int)elapsedMs;
                if (sleep > 0) await Task.Delay(sleep, token);
            }
        }

        #region Internal Logic
        private void InitializeDirectX()
        {
            D3D11.D3D11CreateDevice(adapter: null, driverType: Vortice.Direct3D.DriverType.Hardware, flags: DeviceCreationFlags.None, featureLevels: new[] { Vortice.Direct3D.FeatureLevel.Level_11_0 }, device: out _device, immediateContext: out _context);
            if (_device == null) throw new NotSupportedException("Cannot create D3D11 device.");
            using (var dxgiDevice = _device.QueryInterface<IDXGIDevice>())
            using (var adapter = dxgiDevice.GetAdapter())
            {
                IDXGIOutput? output = null;
                adapter.EnumOutputs(0, out output);
                using (output)
                using (var output1 = output.QueryInterface<IDXGIOutput1>())
                {
                    _duplication = output1.DuplicateOutput(_device);
                    var desc = output.Description;
                    _screenWidth = desc.DesktopCoordinates.Right - desc.DesktopCoordinates.Left;
                    _screenHeight = desc.DesktopCoordinates.Bottom - desc.DesktopCoordinates.Top;
                }
            }
        }

        private Bitmap? CaptureFrame()
        {
            IDXGIResource? screenResource = null;
            try
            {
                var result = _duplication!.AcquireNextFrame(100, out _, out screenResource);
                if (result.Failure || screenResource == null) return null;
                using (screenResource)
                using (var texture = screenResource.QueryInterface<ID3D11Texture2D>())
                    return TextureToBitmap(texture);
            }
            finally { _duplication?.ReleaseFrame(); }
        }

        private Bitmap TextureToBitmap(ID3D11Texture2D texture)
        {
            var desc = texture.Description;
            desc.Usage = ResourceUsage.Staging;
            desc.CPUAccessFlags = CpuAccessFlags.Read;
            desc.BindFlags = BindFlags.None;
            desc.MiscFlags = ResourceOptionFlags.None;
            using (var stagingTexture = _device!.CreateTexture2D(desc))
            {
                _context!.CopyResource(stagingTexture, texture);
                var map = _context.Map(stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
                var bmp = new Bitmap(_screenWidth, _screenHeight, PixelFormat.Format32bppArgb);
                var bmpData = bmp.LockBits(new Rectangle(0, 0, _screenWidth, _screenHeight), ImageLockMode.WriteOnly, bmp.PixelFormat);
                unsafe { Buffer.MemoryCopy((void*)map.DataPointer, (void*)bmpData.Scan0, bmpData.Stride * _screenHeight, bmpData.Stride * _screenHeight); }
                bmp.UnlockBits(bmpData);
                _context.Unmap(stagingTexture, 0);
                return bmp;
            }
        }

        private Bitmap Resize(Bitmap original)
        {
            if (original.Width == _targetWidth && original.Height == _targetHeight) return new Bitmap(original);
            var resized = new Bitmap(_targetWidth, _targetHeight);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, _targetWidth, _targetHeight);
            }
            return resized;
        }

        private byte[] Encode(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, _jpegCodec, _encoderParams);
                return ms.ToArray();
            }
        }

        private static ImageCodecInfo? GetJpegCodec() => ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
        #endregion

        public void Dispose()
        {
            _duplication?.Dispose();
            _context?.Dispose();
            _device?.Dispose();
            _encoderParams?.Dispose();
        }
    }
}