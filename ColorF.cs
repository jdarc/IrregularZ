namespace IrregularZ
{
    public struct ColorF
    {
        public float Red;
        public float Grn;
        public float Blu;

        public static ColorF FromRgb(byte red, byte green, byte blue)
        {
            return FromRgb((red << 0x10) | (green << 0x08) | blue);
        }

        public static ColorF FromRgb(int rgb)
        {
            return FromXyz(((rgb >> 0x10) & 0xFF) / 256.0f,
                ((rgb >> 0x08) & 0xFF) / 256.0f,
                ((rgb >> 0x00) & 0xFF) / 256.0f);
        }

        public static ColorF FromXyz(float r, float g, float b)
        {
            return new ColorF
            {
                Red = Function.Clamp(r),
                Grn = Function.Clamp(g),
                Blu = Function.Clamp(b)
            };
        }
    }
}