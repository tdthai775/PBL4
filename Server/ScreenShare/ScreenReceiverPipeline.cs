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

        public void SetTargetClient(string? clientId) => _streamingClientId = clientId;

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
                if (buffer.Chunks.Count == buffer.ExpectedChunks && buffer.ExpectedChunks > 0)
                {
                    AssembleAndProcessFrame(buffer, clientId);
                }
            }
            catch { buffer.Chunks.Clear(); }
        }

        private void AssembleAndProcessFrame(ClientFrameBuffer buffer, string clientId)
        {
            if (clientId != _streamingClientId) { buffer.Chunks.Clear(); return; }
            using (var ms = new MemoryStream())
            {
                for (int i = 0; i < buffer.ExpectedChunks; i++)
                {
                    if (!buffer.Chunks.TryGetValue(i, out var chunkData)) { buffer.Chunks.Clear(); return; }
                    ms.Write(chunkData, 0, chunkData.Length);
                }
                byte[] jpegData = ms.ToArray();
                buffer.Chunks.Clear();
                using (var bmp = Decode(jpegData))
                {
                    if (bmp != null) FrameReady?.Invoke(this, (clientId, new Bitmap(bmp)));
                }
            }
        }

        private Bitmap? Decode(byte[] jpegData)
        {
            if (jpegData == null || jpegData.Length == 0) return null;
            try { using (var ms = new MemoryStream(jpegData)) { return new Bitmap(ms); } }
            catch { return null; }
        }
    }
}