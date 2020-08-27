using System.IO;
using System.Text;
using static System.IO.Path;

namespace IrregularZ
{
    public sealed class ResourceLoader
    {
        public string Path { get; set; }

        public byte[] LoadResource(string name)
        {
            return File.ReadAllBytes(Combine(Path, name));
        }

        public Model LoadModel(string name)
        {
            var directives = Encoding.ASCII.GetString(LoadResource(name));
            var wavefrontMaterialLoader = new WavefrontMaterialLoader(this, directives);
            return new WavefrontLoader().LoadModel(wavefrontMaterialLoader, directives);
        }
    }
}