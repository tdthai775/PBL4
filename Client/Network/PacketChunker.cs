using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Network
{
    internal class PacketChunker
    {
        private readonly int _chunkSize;
        private int _currentFrameId = 0;

        public PacketChunker(int chunkSize = 1400)
        {
            _chunkSize = chunkSize;
        }

        public List<byte[]> CreateFullFrameChunks(byte[] fullFrameData)
        {
            var chunks = new List<byte[]>();
            int numChunks = (int)Math.Ceiling((double)fullFrameData.Length / _chunkSize);
            if (numChunks == 0) numChunks = 1;

            for (int i = 0; i < numChunks; i++)
            {
                int offset = i * _chunkSize;
                int size = Math.Min(_chunkSize, fullFrameData.Length - offset);
                chunks.Add(CreateChunkPacket(_currentFrameId, i, numChunks, fullFrameData, offset, size));
            }
            _currentFrameId = (_currentFrameId + 1) % int.MaxValue;
            return chunks;
        }

        private byte[] CreateChunkPacket(int frameId, int chunkIndex, int totalChunks, byte[] data, int offset, int size)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(frameId);
                bw.Write(chunkIndex);
                bw.Write(totalChunks);
                bw.Write(size);
                bw.Write(data, offset, size);
                return ms.ToArray();
            }
        }
    }
}
