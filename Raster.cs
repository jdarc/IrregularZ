using System;

namespace IrregularZ
{
    public class Raster : IDisposable
    {
        private const int Opaque = 0xFF << 24;

        public readonly DataBuffer Buffer;
        public readonly int Height;
        public readonly int Width;

        public Raster(int width, int height)
        {
            Width = width;
            Height = height;
            Buffer = new DataBuffer(width * height * sizeof(int));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Raster()
        {
            Dispose(false);
        }

        public void Clear(int color)
        {
            Buffer.Fill(color);
        }

        public void ReOrder()
        {
            unsafe
            {
                var mem = (int*) Buffer.Scan0;
                var index = Buffer.Size >> 2;
                while (index-- > 0) *mem++ = Swizzle(*mem);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing) Buffer.Dispose();
        }

        private static int Swizzle(int argb)
        {
            return (Opaque & argb) | ((argb << 16) & 0xFF0000) | (argb & 0xFF00) | ((argb >> 16) & 0xFF);
        }
    }
}