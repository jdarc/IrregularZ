namespace IrregularZ
{
    public class Material
    {
        public ColorF Ambient;
        public ColorF Diffuse;

        public Material()
        {
            Ambient = ColorF.FromRgb(0x080808);
            Diffuse = ColorF.FromRgb(0xFFFFFF);
        }
    }
}