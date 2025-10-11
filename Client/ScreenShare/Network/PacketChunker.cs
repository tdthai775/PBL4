using System;
using System.Collections.Generic;
using System.IO;

namespace Client.ScreenShare.Network
{
    /// <summary>
    /// Chia dữ liệu JPEG thành các chunks nhỏ để gửi qua UDP
    /// </summary>
    internal class PacketChunker
    {
        private readonly int _chunkSize;
        private int _currentFrameId = 0;

        // Chunk size tối đa (tránh vượt MTU ~1500 bytes)
        public PacketChunker(int chunkSize = 1200)
        {
            _chunkSize = chunkSize;
        }

        /// <summary>
        /// Chia JPEG data thành các chunks
        /// </summary>
        public List<byte[]> CreateChunks(byte[] jpegData)
        {
            var chunks = new List<byte[]>();
            int totalSize = jpegData.Length;
            int numChunks = (int)Math.Ceiling((double)totalSize / _chunkSize);

            for (int i = 0; i < numChunks; i++)
            {
                int offset = i * _chunkSize;
                int size = Math.Min(_chunkSize, totalSize - offset);

                // Tạo chunk packet với header
                byte[] chunk = CreateChunkPacket(_currentFrameId, i, numChunks, jpegData, offset, size);
                chunks.Add(chunk);
            }

            // Tăng frame ID cho frame tiếp theo
            _currentFrameId++;

            return chunks;
        }

        /// <summary>
        /// Tạo chunk packet với header:
        /// [frameId(4)] [chunkIdx(4)] [numChunks(4)] [dataSize(4)] [data(size)]
        /// </summary>
        private byte[] CreateChunkPacket(int frameId, int chunkIdx, int numChunks,
            byte[] data, int offset, int size)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Header
                bw.Write(frameId);      // 4 bytes
                bw.Write(chunkIdx);     // 4 bytes
                bw.Write(numChunks);    // 4 bytes
                bw.Write(size);         // 4 bytes

                // Data
                bw.Write(data, offset, size);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Parse chunk packet để lấy thông tin
        /// </summary>
        public static ChunkInfo ParseChunk(byte[] chunkPacket)
        {
            using (var ms = new MemoryStream(chunkPacket))
            using (var br = new BinaryReader(ms))
            {
                var info = new ChunkInfo();
                info.FrameId = br.ReadInt32();
                info.ChunkIndex = br.ReadInt32();
                info.TotalChunks = br.ReadInt32();
                info.DataSize = br.ReadInt32(); // Đọc và lưu kích thước
                info.Data = br.ReadBytes(info.DataSize); // Sử dụng kích thước đã lưu

                return info;
            }
        }

        /// <summary>
        /// Thông tin chunk
        /// </summary>
        public class ChunkInfo
        {
            public int FrameId { get; set; }
            public int ChunkIndex { get; set; }
            public int TotalChunks { get; set; }
            public int DataSize { get; set; }
            public byte[]? Data { get; set; }
        }
    }
}