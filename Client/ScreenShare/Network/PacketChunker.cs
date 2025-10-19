using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Client.ScreenShare.Network
{
    internal class PacketChunker
    {
        private readonly int _chunkSize;
        private int _currentFrameId = 0;

        public PacketChunker(int chunkSize = 1200) { _chunkSize = chunkSize; }

        public List<byte[]> CreatePatchChunks(byte[] patchData, Rectangle rect)
        {
            var chunks = new List<byte[]>();
            int numChunks = (int)Math.Ceiling((double)patchData.Length / _chunkSize);

            for (int i = 0; i < numChunks; i++)
            {
                int offset = i * _chunkSize;
                int size = Math.Min(_chunkSize, patchData.Length - offset);
                chunks.Add(CreateChunkPacket(true, _currentFrameId, i, numChunks, rect, patchData, offset, size));
            }

            _currentFrameId++;
            return chunks;
        }

        private byte[] CreateChunkPacket(bool isPatch, int frameId, int chunkIndex, int totalChunks,
            Rectangle rect, byte[] data, int offset, int size)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(isPatch);
                bw.Write(frameId);
                bw.Write(chunkIndex);
                bw.Write(totalChunks);

                if (isPatch)
                {
                    bw.Write(rect.X);
                    bw.Write(rect.Y);
                    bw.Write(rect.Width);
                    bw.Write(rect.Height);
                }

                bw.Write(size);
                bw.Write(data, offset, size);

                return ms.ToArray();
            }
        }
    }
}