using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Client.ScreenShare.Capture;
using Client.ScreenShare.Encoder;
using Client.ScreenShare.Network;

namespace Client.ScreenShare.Pipeline
{
    internal class ScreenSenderPipeline : IDisposable
    {
        private readonly string _remoteIp;
        private readonly int _remotePort;
        private readonly int _jpegQuality;
        private readonly int _targetFps;

        private ScreenCapture? _capture;
        private JpegEncoder? _encoder;
        private PacketChunker? _chunker;
        private UdpSender? _sender;

        private CancellationTokenSource? _cts;
        private Task? _pipelineTask;
        private bool _isRunning = false;

        private int _framesProcessed = 0; 
        private readonly Stopwatch _statsWatch = new Stopwatch();

        public ScreenSenderPipeline(string remoteIp, int remotePort, int jpegQuality = 50, int targetFps = 30)
        {
            _remoteIp = remoteIp;
            _remotePort = remotePort;
            _jpegQuality = jpegQuality;
            _targetFps = Math.Max(1, targetFps);
        }

        public void Start()
        {
            if (_isRunning) return;
            if (!InitializeComponents()) return;

            _isRunning = true;
            _cts = new CancellationTokenSource();
            _framesProcessed = 0;
            _statsWatch.Restart();
            _pipelineTask = Task.Run(PipelineLoop, _cts.Token);
            Console.WriteLine(">> DÂY CHUYỀN BẮT ĐẦU HOẠT ĐỘNG! <<");
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cts?.Cancel();
            try { _pipelineTask?.Wait(1000); } catch { }
            _statsWatch.Stop();
            PrintStatistics();
            DisposeComponents();
        }

        private bool InitializeComponents()
        {
            Console.WriteLine("Pipline: Đang chuẩn bị dây chuyền...");
            try
            {
                _capture = new ScreenCapture();
                _encoder = new JpegEncoder(_jpegQuality);
                _chunker = new PacketChunker();
                _sender = new UdpSender(_remoteIp, _remotePort);
                Console.WriteLine("Pipline: Dây chuyền sẵn sàng!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pipline: Lỗi khi thiết lập nhà máy! - {ex.Message}");
                return false;
            }
        }

        private async Task PipelineLoop()
        {
            int frameDelay = 1000 / _targetFps;

            while (_cts != null && !_cts.IsCancellationRequested)
            {
                long startTimeTicks = Stopwatch.GetTimestamp();
                try
                {
                    // STEP 1: Ra lệnh cho Anh Chụp Ảnh
                    var captureResult = _capture?.CaptureFrameWithDirtyRects();
                    if (captureResult == null || !captureResult.HasChanges)
                    {
                        await Task.Delay(10);
                        continue;
                    }

                    // Lấy ra bức ảnh LỚN để làm nguồn cắt
                    using (Bitmap fullFrame = captureResult.Frame)
                    {
                        // STEP 2: Xử lý TỪNG MẢNH VÁ trong báo cáo dirtyRects
                        foreach (var rect in captureResult.DirtyRects)
                        {
                            if (rect.Width <= 0 || rect.Height <= 0) continue;

                            // 2a. Dùng "kéo" cắt ra một miếng ảnh nhỏ (patch)
                            using (Bitmap patch = fullFrame.Clone(rect, fullFrame.PixelFormat))
                            {
                                // 2b. Đưa miếng vá này đi nén
                                byte[] jpegData = _encoder!.Encode(patch);

                                // 2c. Đưa cho Bác Đóng Gói để tạo gói hàng có tọa độ
                                var chunks = _chunker!.CreatePatchChunks(jpegData, rect);

                                // 2d. Đưa cho Chú Giao Hàng để gửi các chunk của miếng vá
                                _sender!.SendChunks(chunks);
                            }
                        }
                    }
                    _framesProcessed++;
                }
                catch (DirectXDeviceLostException)
                {
                    Console.WriteLine("QUẢN ĐỐC: Mất kết nối GPU! Đang khởi động lại trạm chụp ảnh...");
                    _capture?.Dispose();
                    _capture = new ScreenCapture();
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LỖI DÂY CHUYỀN: {ex.Message}");
                    await Task.Delay(100);
                }

                // Điều chỉnh tốc độ
                double elapsedMs = (Stopwatch.GetTimestamp() - startTimeTicks) * 1000.0 / Stopwatch.Frequency;
                int sleepDuration = frameDelay - (int)elapsedMs;
                if (sleepDuration > 0)
                {
                    await Task.Delay(sleepDuration);
                }
            }
        }

        private void PrintStatistics()
        {
            if (_statsWatch.Elapsed.TotalSeconds < 1) return;
            double elapsedSec = _statsWatch.Elapsed.TotalSeconds;
            double fps = _framesProcessed / elapsedSec;
            double bandwidth = _sender?.GetBandwidthKBps(elapsedSec) ?? 0;
            Console.WriteLine($"[CLIENT] Frames: {_framesProcessed} | FPS: {fps:F1} | Bandwidth: {bandwidth:F1} KB/s");
        }

        private void DisposeComponents()
        {
            _capture?.Dispose();
            _encoder?.Dispose();
            _sender?.Dispose();
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}