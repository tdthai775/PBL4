using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Server.ScreenShare.Core
{
    public class ScreenReceiverPipeline
    {
        public event EventHandler<(string clientId, Bitmap frame)>? FrameReady;
        private readonly ConcurrentDictionary<string, ClientFrameBuffer> _clientBuffers = new ConcurrentDictionary<string, ClientFrameBuffer>();

        private class ClientFrameBuffer
        {
            public Dictionary<int, byte[]> Chunks { get; } = new Dictionary<int, byte[]>();
            public int CurrentFrameId { get; set; } = -1;
            public int ExpectedChunks { get; set; } = 0;
        }

        private string? _streamingClientId;

        public ScreenReceiverPipeline() { }

        public void SetTargetClient(string? clientId)
        {
            _streamingClientId = clientId;
            Console.WriteLine($"[PIPELINE] *** TARGET SET: {clientId} ***");
        }

        public void ProcessPacket(byte[] packet, string clientId)
        {
            var buffer = _clientBuffers.GetOrAdd(clientId, new ClientFrameBuffer());
            try
            {
                using var ms = new MemoryStream(packet);
                using var br = new BinaryReader(ms);
                int frameId = br.ReadInt32();
                int chunkIndex = br.ReadInt32();
                int totalChunks = br.ReadInt32();

                if (frameId < buffer.CurrentFrameId) return;

                if (frameId > buffer.CurrentFrameId)
                {
                    buffer.Chunks.Clear();
                    buffer.CurrentFrameId = frameId;
                    buffer.ExpectedChunks = totalChunks;
                }

                int dataSize = br.ReadInt32();
                buffer.Chunks[chunkIndex] = br.ReadBytes(dataSize);

                // Chỉ log mỗi 10 chunks để giảm spam
                if (chunkIndex % 10 == 0 || chunkIndex == totalChunks - 1)
                {
                    Console.WriteLine($"[PIPELINE] Client {clientId}: Frame {frameId}, Chunk {chunkIndex}/{totalChunks}");
                }

                if (buffer.Chunks.Count == buffer.ExpectedChunks && buffer.ExpectedChunks > 0)
                {
                    Console.WriteLine($"[PIPELINE] ✓ All {buffer.ExpectedChunks} chunks received for frame {frameId}");
                    AssembleAndProcessFrame(buffer, clientId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PIPELINE] ✗ ERROR processing packet: {ex.Message}");
                buffer.Chunks.Clear();
            }
        }

        private void AssembleAndProcessFrame(ClientFrameBuffer buffer, string clientId)
        {
           
            if (!IsSameClient(clientId, _streamingClientId))
            {
                Console.WriteLine($"[PIPELINE] Frame from {GetIpOnly(clientId)} ignored (not target)");
                buffer.Chunks.Clear();
                return;
            }

            Console.WriteLine($"[PIPELINE] → Processing frame from {GetIpOnly(clientId)}");

            try
            {
                using (var ms = new MemoryStream())
                {
                    for (int i = 0; i < buffer.ExpectedChunks; i++)
                    {
                        if (!buffer.Chunks.TryGetValue(i, out var chunkData))
                        {
                            Console.WriteLine($"[PIPELINE] ✗ Missing chunk {i}");
                            buffer.Chunks.Clear();
                            return;
                        }
                        ms.Write(chunkData, 0, chunkData.Length);
                    }

                    byte[] jpegData = ms.ToArray();
                    buffer.Chunks.Clear();

                    Console.WriteLine($"[PIPELINE] → Assembled {jpegData.Length} bytes, decoding...");

                    using (var bmp = Decode(jpegData))
                    {
                        if (bmp != null)
                        {
                            Console.WriteLine($"[PIPELINE] ✓ Decoded {bmp.Width}x{bmp.Height}, raising event");
                            FrameReady?.Invoke(this, (clientId, new Bitmap(bmp)));
                        }
                        else
                        {
                            Console.WriteLine($"[PIPELINE] ✗ Decode failed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PIPELINE] ✗ Error: {ex.Message}");
                buffer.Chunks.Clear();
            }
        }

        private bool IsSameClient(string clientId1, string? clientId2)
        {
            if (string.IsNullOrEmpty(clientId2)) return false;

            string ip1 = GetIpOnly(clientId1);
            string ip2 = GetIpOnly(clientId2);

            return ip1 == ip2;
        }

        private string GetIpOnly(string clientId)
        {
            if (string.IsNullOrEmpty(clientId)) return "";
            int colonIndex = clientId.IndexOf(':');
            return colonIndex > 0 ? clientId.Substring(0, colonIndex) : clientId;
        }

        private Bitmap? Decode(byte[] jpegData)
        {
            if (jpegData == null || jpegData.Length == 0) return null;
            try
            {
                using (var ms = new MemoryStream(jpegData))
                {
                    return new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PIPELINE] Decode error: {ex.Message}");
                return null;
            }
        }
    }
}