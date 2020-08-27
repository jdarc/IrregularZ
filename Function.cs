namespace IrregularZ
{
    public static class Function
    {
        public static float Clamp(float value, float min = 0, float max = 1)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static int ClampByte(int value)
        {
            return value < 0 ? 0 : value > 0xFF ? 0xFF : value;
        }

        public static int Ceil(float value)
        {
            return 0x3FFFFFFF - (int) (1073741823.0 - value);
        }

        public static int Floor(float value)
        {
            return (int) (1073741823.0 + value) - 0x3FFFFFFF;
        }

        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }

        public static float Min(float a, float b, float c)
        {
            return Min(a, Min(b, c));
        }

        public static float Max(float a, float b, float c)
        {
            return Max(a, Max(b, c));
        }
    }
}