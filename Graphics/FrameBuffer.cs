using System;

namespace IrregularZ.Graphics
{
    public sealed class FrameBuffer<T> where T : struct
    {
        public readonly int Width;
        public readonly int Height;
        public readonly T[] Data;

        public FrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new T[width * height];
        }

        public int Size => Data.Length;

        public T this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public void Fill(T value) => Array.Fill(Data, value);
    }
}