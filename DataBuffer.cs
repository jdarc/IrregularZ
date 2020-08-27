using System;
using System.Runtime.InteropServices;

namespace IrregularZ
{
    public sealed class DataBuffer : IDisposable
    {
        public readonly byte[] Data;
        public readonly IntPtr Scan0;
        private bool _disposed;
        private GCHandle _handle;

        public DataBuffer(int size)
        {
            Data = new byte[size];
            _handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
            Scan0 = _handle.AddrOfPinnedObject();
        }

        public int Size => Data.Length;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DataBuffer()
        {
            Dispose(false);
        }

        public void Fill(float value)
        {
            unsafe
            {
                Fill(*(int*) &value);
            }
        }

        public void Fill(int value)
        {
            unsafe
            {
                var index = Size >> 2;
                var mem = (int*) Scan0;
                while (index-- > 0) *mem++ = value;
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _handle.Free();
            _disposed = true;
        }
    }
}