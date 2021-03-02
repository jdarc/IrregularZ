using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace IrregularZ.Graphics
{
    internal sealed class MaterialsLoader
    {
        private readonly Dictionary<string, Color[]> _materials;

        public MaterialsLoader(string directives)
        {
            directives = directives.Replace("\r\n", "\n");
            _materials = new Dictionary<string, Color[]>();
            var index = directives.IndexOf("mtllib", StringComparison.Ordinal);
            if (index == -1) return;
            var start = directives.IndexOf(' ', index);
            var end = directives.IndexOf(Environment.NewLine, start, StringComparison.Ordinal);
            var mtlibName = directives.Substring(start, end - start).Trim();
            var s = Encoding.ASCII.GetString(ContentLoader.ReadAllBytes(mtlibName));
            Bucketize(Normalize(s)).ForEach(Extract);
        }

        public Color? FindByName(string name) => _materials.ContainsKey(name) ? _materials[name][0] : new Color?();

        public void AddCustomMaterial(string name, Color material) => _materials.Add(name, new[] {material});

        private void Extract(IEnumerable<string> bucket)
        {
            var material = new[] {new Color()};
            foreach (var line in bucket)
            {
                var fragments = line.Split();
                var key = fragments[0];

                if (key.StartsWith("newmtl", StringComparison.InvariantCultureIgnoreCase))
                {
                    _materials.Add(fragments[1], material);
                }
                else if (key.StartsWith("kd", StringComparison.InvariantCultureIgnoreCase))
                {
                    material[0] = ExtractColor(fragments);
                }
            }
        }

        private static Color ExtractColor(IReadOnlyList<string> fragments)
        {
            var red = (int) (float.Parse(fragments[1]) * 255);
            var grn = (int) (float.Parse(fragments[2]) * 255);
            var blu = (int) (float.Parse(fragments[3]) * 255);
            return Color.FromArgb(red, grn, blu);
        }

        private static List<List<string>> Bucketize(string directives)
        {
            var buckets = new List<List<string>>();
            if (string.IsNullOrEmpty(directives) || !directives.Contains("newmtl")) return buckets;

            using var reader = new StringReader(directives);
            var bucket = new List<string>();
            var line = reader.ReadLine();
            while (line != null)
            {
                if (line.StartsWith("newmtl", StringComparison.InvariantCultureIgnoreCase))
                {
                    buckets.Add(bucket = new List<string>());
                }

                bucket.Add(line);
                line = reader.ReadLine();
            }

            return buckets;
        }

        private static string Normalize(string directives)
        {
            var lines = directives.Split(Environment.NewLine);
            return string.Join(Environment.NewLine, lines.Where(it => it.Length > 0 && it[0] != '#')).Trim();
        }
    }
}