using System;
using System.IO;
using System.Text.RegularExpressions;

namespace IrregularZ
{
    public sealed class WavefrontLoader
    {
        private static readonly Regex VertexRegex = new Regex("v (.*) (.*) (.*)", RegexOptions.IgnoreCase);
        private static readonly Regex FaceRegEx = new Regex("f (.*) (.*) (.*)", RegexOptions.IgnoreCase);
        private static readonly Regex UseMaterialRegex = new Regex("usemtl (.*)", RegexOptions.IgnoreCase);
        private static readonly Regex GroupRegex = new Regex("g (.*)", RegexOptions.IgnoreCase);

        public Model LoadModel(WavefrontMaterialLoader materialLoader, string directives)
        {
            var model = new Model();
            var material = new Material();
            using var br = new StringReader(directives);
            var line = br.ReadLine();
            while (line != null)
            {
                if (line.Length > 0)
                {
                    var type = line.Substring(0, line.IndexOf(' ')).Trim().ToLower();
                    if (type.Equals("v"))
                    {
                        var match = VertexRegex.Match(line.Trim());
                        var x = float.Parse(match.Groups[1].Value);
                        var y = float.Parse(match.Groups[2].Value);
                        var z = float.Parse(match.Groups[3].Value);
                        model.AddVertex(x, y, z);
                    }
                    else if (type.Equals("f"))
                    {
                        LoadFace(model, line.Trim(), material);
                    }
                    else if (type.Equals("g"))
                    {
                        model.CurrentGroup = GroupRegex.Match(line.Trim()).Groups[1].Value;
                    }
                    else if (type.Equals("usemtl"))
                    {
                        material = UseMaterial(materialLoader, line.Trim());
                    }
                }

                line = br.ReadLine();
            }

            return model;
        }

        private static void LoadFace(Model model, string line, Material material)
        {
            var vi = 0;
            var v = new int[16];
            var match = FaceRegEx.Match(line);
            for (var i = 1; i < match.Groups.Count; i++)
                v[vi++] = int.Parse(ExtractVertexIndex(match.Groups[i].Value)) - 1;

            model.CreateTriangle(v[0], v[1], v[2]).ChangeMaterial(material);
            for (var i = 1; i < vi - 2; i++) model.CreateTriangle(v[0], v[i + 1], v[i + 2]).ChangeMaterial(material);
        }

        private static string ExtractVertexIndex(string token)
        {
            var idx = token.IndexOf("/", StringComparison.Ordinal);
            return idx == -1 ? token : token.Substring(0, idx);
        }

        private static Material UseMaterial(WavefrontMaterialLoader materialLoader, string line)
        {
            var node = UseMaterialRegex.Match(line).Groups[1].Value;
            var material = materialLoader.FindByName(node);
            if (material == null)
            {
                var rgbNode = node;
                for (var c = '0'; c <= '9'; c++) rgbNode = rgbNode.Replace(c, '-');

                if (rgbNode.IndexOf("R-", StringComparison.Ordinal) != -1 &&
                    rgbNode.IndexOf("G-", StringComparison.Ordinal) != -1 &&
                    rgbNode.IndexOf("-B", StringComparison.Ordinal) != -1)
                {
                    var red = byte.Parse(node.Substring(1, node.IndexOf('G')));
                    var grn = byte.Parse(node.Substring(node.IndexOf('G') + 1, node.IndexOf('B')));
                    var blu = byte.Parse(node.Substring(node.IndexOf('B') + 1));
                    material = new Material {Diffuse = ColorF.FromRgb(red, grn, blu)};
                    materialLoader.AddCustomMaterial(node, material);
                }
                else
                {
                    material = new Material();
                }
            }

            return material;
        }
    }
}