using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IrregularZ
{
    public sealed class WavefrontMaterialLoader
    {
        private readonly Dictionary<string, Material> _materials;

        public WavefrontMaterialLoader(ResourceLoader loader, string directives)
        {
            directives = directives.Replace("\r\n", "\n");
            _materials = new Dictionary<string, Material>();
            var index = directives.IndexOf("mtllib", StringComparison.Ordinal);
            if (index == -1) return;
            var start = directives.IndexOf(' ', index);
            var end = directives.IndexOf(Environment.NewLine, start, StringComparison.Ordinal);
            var mtlibname = directives.Substring(start, end - start).Trim();
            var matdirectives = Encoding.ASCII.GetString(loader.LoadResource(mtlibname));

            MakeBuckets(Normalize(matdirectives)).ForEach(Extract);
        }

        public Material FindByName(string name)
        {
            return _materials.ContainsKey(name) ? _materials[name] : null;
        }

        public void AddCustomMaterial(string name, Material material)
        {
            _materials.Add(name, material);
        }

        private void Extract(IEnumerable<string> bucket)
        {
            var m = new Material();
            foreach (var line in bucket)
            {
                var fragments = line.Split();
                var fragmentKey = fragments[0];
                if (fragmentKey.StartsWith("newmtl", StringComparison.InvariantCultureIgnoreCase))
                    _materials.Add(fragments[1], m = new Material());
                else if (fragmentKey.StartsWith("kd", StringComparison.InvariantCultureIgnoreCase))
                    m.Diffuse = ColorF.FromXyz(float.Parse(fragments[1]), float.Parse(fragments[2]),
                        float.Parse(fragments[3]));
                else if (fragmentKey.StartsWith("ka", StringComparison.InvariantCultureIgnoreCase))
                    m.Ambient = ColorF.FromXyz(float.Parse(fragments[1]), float.Parse(fragments[2]),
                        float.Parse(fragments[3]));
                else if (fragmentKey.StartsWith("ks", StringComparison.InvariantCultureIgnoreCase))
                    ColorF.FromXyz(float.Parse(fragments[1]), float.Parse(fragments[2]), float.Parse(fragments[3]));
            }
        }

        private static List<List<string>> MakeBuckets(string directives)
        {
            var buckets = new List<List<string>>();

            if (!string.IsNullOrEmpty(directives) && directives.IndexOf("newmtl", StringComparison.Ordinal) != -1)
            {
                using var reader = new StringReader(directives);
                var bucket = new List<string>();
                var line = reader.ReadLine();
                while (line != null)
                {
                    if (line.StartsWith("newmtl", StringComparison.InvariantCultureIgnoreCase))
                        buckets.Add(bucket = new List<string>());

                    bucket.Add(line);
                    line = reader.ReadLine();
                }
            }

            return buckets;
        }

        private static string Normalize(string directives)
        {
            using var br = new StringReader(directives);
            var sb = new StringBuilder();
            var nline = br.ReadLine();
            while (nline != null)
            {
                if (nline.Length > 0 && nline[0] != '#')
                {
                    sb.Append(nline.Trim());
                    sb.Append(Environment.NewLine);
                }

                nline = br.ReadLine();
            }

            return sb.ToString().Trim();
        }
    }
}