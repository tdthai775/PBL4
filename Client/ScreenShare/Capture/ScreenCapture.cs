using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography.X509Certificates;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Client.ScreenShare.Capture
{
   public class DirectXDeviceLostException : Exception 
   {
        public DirectXDeviceLostException(string msg) : base(msg) { }
    }

    public class CaptureResult 
    {
        public Bitmap Frame { get; set; } = null!;
        public List<Rectangle> DirtyRects { get; set; } = new List<Rectangle>();
        public bool HasChanges => DirtyRects.Count > 0;
    }

    internal class ScreenCapture : IDisposable
    {
        private ID3D11Device? _device;
        private ID3D11DeviceContext? _context;
        private IDXGIOutputDuplication? _duplication;
        private int _screenWidth;
        private int _screenHeight;

        public ScreenCapture() { InitializeDirectX(); }

        private void InitializeDirectX()
        {
            try
            {
                D3D11.D3D11CreateDevice(
                    adapter: null, 
                    driverType: Vortice.Direct3D.DriverType.Hardware,
                    flags: DeviceCreationFlags.None, 
                    featureLevels: new[] { Vortice.Direct3D.FeatureLevel.Level_11_0 },
                    device: out _device, 
                    immediateContext: out _context);

                if (_device == null) throw new NotSupportedException("Capture: Không thể tạo thiết bị DirectX.");
               
                using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
                using var adapter = dxgiDevice.GetAdapter();
                { 
                    adapter.EnumOutputs(0, out IDXGIOutput output);
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
            catch (Exception ex)
            {
                Dispose();
                throw new Exception("Khởi tạo ScreenCapture thất bại.", ex);
            }
        }

        public CaptureResult? CaptureFrameWithDirtyRects()
        {
            if (_duplication == null) return null;

            try
            {
                var result = _duplication.AcquireNextFrame(100, out var frameInfo, out IDXGIResource screenResource);
                using (screenResource)
                {
                    if (result.Code == Vortice.DXGI.ResultCode.AccessLost.Code) throw new DirectXDeviceLostException("Mất kết nối GPU.");
                    if (result.Failure || screenResource == null || frameInfo.AccumulatedFrames == 0) return null;

                    using var texture = screenResource.QueryInterface<ID3D11Texture2D>();
                    var bitmap = TextureToBitmap(texture);
                    var dirtyRects = GetDirtyRectanglesFromAPI(frameInfo);
                    return new CaptureResult { Frame = bitmap, DirtyRects = dirtyRects };
                }
            }
            finally
            {
                try { _duplication.ReleaseFrame(); } catch {  }
            }
        }

        private List<Rectangle> GetDirtyRectanglesFromAPI(OutduplFrameInfo frameInfo)
        {
            var rects = new List<Rectangle>();
            if (frameInfo.TotalMetadataBufferSize == 0)
            {
                rects.Add(new Rectangle(0, 0, _screenWidth, _screenHeight));
                return rects;
            }

            var dirtyRectsBuffer = new RawRect[frameInfo.TotalMetadataBufferSize / 16];
            _duplication!.GetFrameDirtyRects((uint)(dirtyRectsBuffer.Length * 16), dirtyRectsBuffer, out _);

            foreach (var rect in dirtyRectsBuffer)
            {
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                if (width > 0 && height > 0)
                {
                    rects.Add(new Rectangle(rect.Left, rect.Top, width, height));
                }
            }
            return rects;
        }

        private Bitmap TextureToBitmap(ID3D11Texture2D texture)
        {
            var desc = texture.Description;
            desc.Usage = ResourceUsage.Staging;
            desc.CPUAccessFlags = CpuAccessFlags.Read;
            desc.BindFlags = BindFlags.None;
            desc.MiscFlags = ResourceOptionFlags.None;

            using var stagingTexture = _device!.CreateTexture2D(desc);
            _context!.CopyResource(stagingTexture, texture);
            var dataBox = _context.Map(stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

            var bitmap = new Bitmap(_screenWidth, _screenHeight, PixelFormat.Format32bppArgb);
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, _screenWidth, _screenHeight), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            unsafe
            {
                byte* src = (byte*)dataBox.DataPointer;
                byte* dst = (byte*)bmpData.Scan0;
                for (int y = 0; y < _screenHeight; y++)
                {
                    Buffer.MemoryCopy(src, dst, (long)_screenWidth * 4, (long)_screenWidth * 4);
                    src += dataBox.RowPitch;
                    dst += bmpData.Stride;
                }
            }

            bitmap.UnlockBits(bmpData);
            _context.Unmap(stagingTexture, 0);
            return bitmap;
        }

        public void Dispose()
        {
            _duplication?.Dispose();
            _context?.Dispose();
            _device?.Dispose();
        }
    }

}
