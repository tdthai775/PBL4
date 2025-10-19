using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Server.ScreenShare.Network
{
    public class AssembledFrame
    {
        public byte[] JpegData { get; set; } = null!;
        public Rectangle? PatchLocation { get; set; }
    }

    internal class PacketAssembler
    {
        private readonly Dictionary<int, byte[]> _chunkBuffer = new Dictionary<int, byte[]>();
        private int _currentFrameId = -1;
        private int _expectedChunks = 0;
        private Rectangle? _patchRect;

        public event Action<AssembledFrame>? FrameAssembled;

        public void ProcessPacket(byte[] packet)
        {
            try
            {
                using var ms = new MemoryStream(packet);
                using var br = new BinaryReader(ms);

                bool isPatch = br.ReadBoolean();
                int frameId = br.ReadInt32();
                int chunkIndex = br.ReadInt32();
                int totalChunks = br.ReadInt32();

                if (frameId < _currentFrameId) return; 

                if (frameId > _currentFrameId)
                {
                    _chunkBuffer.Clear();
                    _currentFrameId = frameId;
                    _expectedChunks = totalChunks;
                    _patchRect = null;
                }

                if (isPatch)
                {
                    _patchRect = new Rectangle(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                }

                int dataSize = br.ReadInt32();
                _chunkBuffer[chunkIndex] = br.ReadBytes(dataSize);

                if (_chunkBuffer.Count == _expectedChunks)
                {
                    AssembleAndRaiseEvent();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Loi lap rap goi tin: {ex.Message}"); }
        }

        private void AssembleAndRaiseEvent()
        {
            using (var ms = new MemoryStream())
            {
                for (int i = 0; i < _expectedChunks; i++)
                {
                    if (_chunkBuffer.ContainsKey(i))
                    {
                        ms.Write(_chunkBuffer[i], 0, _chunkBuffer[i].Length);
                    }
                    else
                    {
                        Console.WriteLine($"Thieu manh ghep {i} cua frame {_currentFrameId}!");
                        _chunkBuffer.Clear();
                        return; 
                    }
                }

                FrameAssembled?.Invoke(new AssembledFrame { JpegData = ms.ToArray(), PatchLocation = _patchRect });
            }
            _chunkBuffer.Clear();
        }
    }
}